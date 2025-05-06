using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Handlers.Messages;
using Mediator;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace DobissConnectorService.Consumers
{
    public class LightDimmedConsumer(ILogger<LightDimmedConsumer> logger, LightCacheService lightCacheService, IMediator mediator) : IConsumer<IConsumerContext<LightToggledMessage>>
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

            Light? light = lightCacheService.Get(module, device)
                ?? throw new ArgumentException($"Light with module {module} and device {device} not found");

            await mediator.Send(new ToggleLightMessage(light, StateToInt(message.Message.State)), cancellationToken);
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
