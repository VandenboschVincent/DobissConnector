using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Dobiss;
using DobissConnectorService.Handlers.Messages;
using DobissConnectorService.Services;
using Mediator;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace DobissConnectorService.Handlers
{
    public class ToggleLightMessageHandler(ILogger<ToggleLightMessageHandler> logger, DobissClientFactory dobissClientFactory, LightCacheService lightCacheService, IPublishBus publishBus) : ICommandHandler<ToggleLightMessage>
    {
        public async ValueTask<Unit> Handle(ToggleLightMessage command, CancellationToken cancellationToken)
        {
            DobissService service = dobissClientFactory.Get()
                ?? throw new ArgumentException("Dobiss client is null");
            Light light = command.Light;

            command.NewState ??= light.CurrentValue switch
            {
                100 => 0,
                0 => 100,
                _ => 0
            };

            if (light.CurrentValue != command.NewState)
            {
                await service.ToggleOutput(light.ModuleKey, light.Key, cancellationToken);
                light.CurrentValue = command.NewState.Value;
                lightCacheService.Update(light);
            }
            else
                logger.LogInformation("Light already in state {State}", command.NewState);
            await publishBus.Publish(new LightStateMessage(IntToState(command.NewState.Value)), $"{BackgroundWorker.topicPath}{light.ModuleKey}x{light.Key}/state", null, cancellationToken);
            return Unit.Value;
        }

        private static string IntToState(int state)
        {
            return state switch
            {
                100 => "ON",
                0 => "OFF",
                _ => throw new ArgumentException($"Invalid state: {state}")
            };
        }
    }
}
