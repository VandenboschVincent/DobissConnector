using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss;
using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace DobissConnectorService
{
    public class BackgroundWorker(ILogger<BackgroundWorker> logger, IOptions<DobissSettings> options, IPublishBus publishBus, DobissClientFactory dobissClientFactory, LightCacheService lightCacheService) : BackgroundService
    {
        public const string topicPath = "homeassistant/light/dobiss_";
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogDebug("Starting worker with config: {@Config}", options.Value);

            DobissService dobissService = dobissClientFactory.Create(options.Value.DobissIp, options.Value.DobissPort, options.Value.Modules, logger, lightCacheService);

            //Fetching all lights
            await FetchGroupData(dobissService, stoppingToken);

            //Stop syncing when no delay is set
            if (options.Value.Delay <= 0)
                return;

            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogDebug("Running Dobiss sync");
                //Fetching status of all lights
                await FetchStatus(dobissService, stoppingToken);
                await Task.Delay(options.Value.Delay, stoppingToken);
            }
        }

        private async Task FetchGroupData(DobissService dobissService, CancellationToken cancellationToken)
        {
            foreach (KeyValuePair<int, ModuleType> module in options.Value.Modules)
            {
                logger.LogDebug("Fetching data for module {Module} with type {Type}", module.Key, module.Value);
                List<DobissGroupData> outputData = await dobissService.FetchOutputsData(module.Key, cancellationToken);
                logger.LogDebug("Module {Module} found with data {@Data}", module.Key, outputData);
                foreach(DobissGroupData groupData in outputData)
                {
                    logger.LogInformation("Found light {Light} with address {Address} and module {Module}:{ModuleId}", groupData.name, groupData.id, module.Value, module.Key);
                    lightCacheService.Add(new Light(module.Key, groupData.id, module.Value, groupData.name));
                    await publishBus.Publish(new LightConfigMessage(groupData.name, module.Key, groupData.id), $"{topicPath}{module.Key}x{groupData.id}/config", null, cancellationToken);
                }
            }
        }

        private async Task FetchStatus(DobissService dobissService, CancellationToken cancellationToken)
        {
            List<DobissModule> moduleStatuses = await dobissService.RequestAllStatus(cancellationToken);
            foreach (DobissModule module in moduleStatuses)
            {
                foreach (DobissOutput output in module.outputs)
                {
                    Light? light = lightCacheService.Get(module.module, output.address);
                    if (light is null)
                    {
                        logger.LogWarning("No group data found for module {Module} with address {Address}", module.module, output.address);
                        continue;
                    }
                    logger.LogDebug("Found light {Light} with data {Status}", light.Name, output.status);
                    if (light.CurrentValue != output.status)
                    {
                        light.CurrentValue = output.status;
                        lightCacheService.Update(light);
                        logger.LogInformation("Light {Light} has changed to {Status}", light.Name, output.status);
                    }
                    if (output.status > 1 || output.status < 0)
                        continue;
                    await publishBus.Publish(new LightStateMessage(output.status == 1 ? "ON" : "OFF"), $"{topicPath}{module.module}x{light.Key}/state", null, cancellationToken);
                }
            }
        }
    }
}
