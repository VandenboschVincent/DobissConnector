using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Dobiss;
using DobissConnectorService.Services;
using Mediator;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using DobissConnectorService.CommandHandlers.Commands;

namespace DobissConnectorService.CommandHandlers
{
    public class ToggleLightCommandHandler(ILogger<ToggleLightCommandHandler> logger, DobissClientFactory dobissClientFactory, LightCacheService lightCacheService, IPublishBus publishBus) : ICommandHandler<ChangeLightCommand>
    {
        public async ValueTask<Unit> Handle(ChangeLightCommand command, CancellationToken cancellationToken)
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
            await publishBus.Publish(new LightChangedMessage(command.NewState.Value.ToString()), $"{BackgroundWorker.topicPath}{light.ModuleKey}x{light.Key}/state", null, cancellationToken);
            return Unit.Value;
        }

    }
}
