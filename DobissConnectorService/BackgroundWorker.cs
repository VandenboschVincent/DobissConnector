using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss;
using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace DobissConnectorService
{
    public class BackgroundWorker(ILogger<BackgroundWorker> logger, IOptions<DobissSettings> options, IPublishBus publishBus, IDobissClientFactory dobissClientFactory, LightCacheService lightCacheService) : BackgroundService
    {
        public const string topicPath = "homeassistant/light/dobiss_";
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogDebug("Starting worker with config: {@Config}", options.Value);

            DobissService dobissService = dobissClientFactory.Create(options.Value.DobissIp, options.Value.DobissPort, logger, lightCacheService);
            List<DobissModule> modules = [];
            await using (await dobissService.DobissClient.Connect(stoppingToken))
            {

                //Fetching all modules
                modules = await FetchModules(dobissService, stoppingToken);
                logger.LogInformation("Modules found: {@Modules}", modules);

                //Fetching all lights
                await FetchLights(modules, dobissService, stoppingToken);

                //Sending config for all lights
                await SendConfig(stoppingToken);

                //Stop syncing when no delay is set
                if (options.Value.Delay <= 0)
                {
                    return;
                }
            }

            int i = 0;
            await Task.Delay(1000, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                i++;
                await using (await dobissService.DobissClient.Connect(stoppingToken))
                {
                    logger.LogDebug("Running Dobiss sync {Counter}", i);
                    //Fetching status of all lights
                    await FetchStatus(modules, dobissService, stoppingToken);
                }
                if (i % 50 == 0)
                {
                    //Resend config every 50 iterations
                    await SendConfig(stoppingToken);
                }
                await Task.Delay(options.Value.Delay, stoppingToken);
            }
        }

        private static async Task<List<DobissModule>> FetchModules(DobissService dobissService, CancellationToken cancellationToken)
        {
            return await dobissService.FetchModules(cancellationToken);
        }

        private async Task FetchLights(List<DobissModule> modules, DobissService dobissService, CancellationToken cancellationToken)
        {
            foreach (DobissModule module in modules)
            {
                logger.LogDebug("Fetching data for module {Module} with type {Type}", module.Index, module.Type);
                List<DobissOutput> outputData = await dobissService.FetchOutputsData(module, cancellationToken);
                logger.LogDebug("Module {Module} found with data {@Data}", module.Index, outputData);
                foreach(DobissOutput light in outputData)
                {
                    logger.LogInformation("Found light {Light} {Module}:{ModuleId} with address {Address} and type {Type}", light.Name, light.Index, module.Type, module.Index, light.Type);
                    lightCacheService.Add(new Light(module.Index, light.Index, module.Type, light.Name, light.Type));
                }
            }
        }

        private async Task SendConfig(CancellationToken cancellationToken)
        {
            var lights = lightCacheService.GetAll();
            foreach (Light light in lights)
            {
                await publishBus.Publish(
                    light.ModuleType == ModuleType.DIMMER
                        ? new DimLightConfigMessage(light.Name, light.ModuleKey, light.Key)
                        : new LightConfigMessage(light.Name, light.ModuleKey, light.Key), $"{topicPath}{light.ModuleKey}x{light.Key}/config", null, cancellationToken);
            }
        }

        private async Task FetchStatus(List<DobissModule> modules, DobissService dobissService, CancellationToken cancellationToken)
        {
            var moduleStatuses = await dobissService.RequestAllStatus(modules, cancellationToken).ToListAsync(cancellationToken);
            foreach (var (moduleIndex, index, value) in moduleStatuses)
            {
                Light? light = lightCacheService.Get(moduleIndex, index);
                if (light is null)
                {
                    logger.LogWarning("No group data found for module {Module} with address {Address}", moduleIndex, index);
                    continue;
                }
                int outputStatus = value == 1 ? 100 : value;
                logger.LogDebug("Found light {Light} with data {Status}", light.Name, outputStatus);
                if (light.CurrentValue != outputStatus)
                {
                    light.CurrentValue = outputStatus;
                    lightCacheService.Update(light);
                    logger.LogInformation("Light {Light} has changed to {Status}", light.Name, outputStatus);
                }
                if (outputStatus == 0 || outputStatus == 100)
                    await publishBus.Publish(new LightChangedMessage(outputStatus == 100 ? "ON" : "OFF", null), $"{topicPath}{light.ModuleKey}x{light.Key}/state", null, cancellationToken);
                else
                    await publishBus.Publish(new LightChangedMessage("ON", outputStatus), $"{topicPath}{light.ModuleKey}x{light.Key}/state", null, cancellationToken);
            }
        }
    }
}
