using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss;
using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Services;
using SlimMessageBus;

namespace DobissConnectorService.Consumers
{
    public class LightToggledConsumer(DobissClientFactory dobissClientFactory, IPublishBus publishBus, ILogger<LightToggledConsumer> logger) : IConsumer<IConsumerContext<LightToggledMessage>>
    {
        public async Task OnHandle(IConsumerContext<LightToggledMessage> message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handle message");
            string? path = message.Headers["origPath"].ToString();
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path is null or empty");
            }
            int module = path[^3] - '0';
            int device = path[^1] - '0';

            Light? light = Worker.lights.FirstOrDefault(x => x.ModuleKey == module && x.Key == device) 
                ?? throw new ArgumentException($"Light with module {module} and device {device} not found");
            DobissService service = dobissClientFactory.Get()
                ?? throw new ArgumentException("Dobiss client is null");

            if (light.CurrentValue != StateToInt(message.Message.State))
            {
                await service.ToggleOutput(light.ModuleKey, light.Key, cancellationToken);
                light.CurrentValue = StateToInt(message.Message.State);
            }
            else
                logger.LogInformation("Light already in state {State}", message.Message.State);
            await publishBus.Publish(new LightStateMessage(message.Message.State), $"{Worker.topicPath}{module}x{device}/state", null, cancellationToken);

        }

        private static int StateToInt(string state)
        {
            return state switch
            {
                "ON" => 1,
                "OFF" => 0,
                _ => throw new ArgumentException($"Invalid state: {state}")
            };
        }
    }
}
