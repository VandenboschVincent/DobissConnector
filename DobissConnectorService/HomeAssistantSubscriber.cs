using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Models;
using Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using ToMqttNet;

namespace DobissConnectorService
{
    public class HomeAssistantSubscriber(ILogger<HomeAssistantSubscriber> logger, IMqttConnectionService mqttClient, IMediator mediator) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("Starting subscriber with options {@Options}", mqttClient.MqttOptions);
                await mqttClient.SubscribeAsync(new MQTTnet.Packets.MqttTopicFilter()
                {
                    Topic = "homeassistant/status",
                    QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce
                });
                mqttClient.OnApplicationMessageReceivedAsync += async (message) =>
                {
                    if (message.ApplicationMessage.Topic.StartsWith("homeassistant/status"))
                    {
                        var value = Encoding.UTF8.GetString(message.ApplicationMessage.PayloadSegment);
                        logger.LogDebug("HomeAssistant status changed to {Status}", value);
                        if (value is not null && value.Contains("online", StringComparison.OrdinalIgnoreCase))
                        {
                            logger.LogInformation("HomeAssistant is online, sending discovery documents.");
                            await SendConfig(stoppingToken);
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in HomeAssistantSubscriber: {Message}", ex.Message);
            }
        }

        private async Task SendConfig(CancellationToken cancellationToken)
        {
            var lights = await lightCacheService.GetAll();
            foreach (Light light in lights)
            {
                await publishBus.Publish(
                    light.ModuleType == ModuleType.DIMMER
                        ? new DimLightConfigMessage(light.Name, light.ModuleKey, light.Key, options.CurrentValue.DeviceName)
                        : new LightConfigMessage(light.Name, light.ModuleKey, light.Key, options.CurrentValue.DeviceName)
                        , $"{BackgroundWorker.topicPath}{light.ModuleKey}x{light.Key}/config", null, cancellationToken);
            }
            logger.LogInformation("Config resend for all lights");
        }
    }
}
