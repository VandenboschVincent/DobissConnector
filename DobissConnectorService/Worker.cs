using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss;
using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Services;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace DobissConnectorService
{
    public class Worker(ILogger<Worker> logger, IOptions<Settings> options, IPublishBus publishBus, DobissClientFactory dobissClientFactory) : BackgroundService
    {
        public const string topicPath = "homeassistant/light/dobiss_";
        public static readonly List<Light> lights =[];
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogDebug("Starting worker with config: {@Config}", options.Value);

            DobissService dobissService = dobissClientFactory.Create(options.Value.DobissIp, options.Value.DobissPort, options.Value.Modules, logger);

            await FetchGroupData(dobissService, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Running Dobiss sync");
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
                    logger.LogInformation("Found light {Light} with address {Address} and module {Module}", groupData.name, groupData.id, module.Value);
                    lights.Add(new Light(module.Key, groupData.id, module.Value, groupData.name));
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
                    Light? groupData = lights.FirstOrDefault(g => g.ModuleKey == module.module && g.Key == output.address);
                    if (groupData is null)
                    {
                        logger.LogWarning("No group data found for module {Module} with address {Address}", module.module, output.address);
                        continue;
                    }
                    logger.LogInformation("Found light {Light} with data {Status}", groupData.Name, output.status);
                    if (output.status > 1 || output.status < 0)
                        continue;
                    groupData.CurrentValue = output.status;
                    await publishBus.Publish(new LightStateMessage(output.status == 1 ? "ON" : "OFF"), $"{topicPath}{module.module}x{groupData.Key}/state", null, cancellationToken);
                }
            }
        }
    }
}
