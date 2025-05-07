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
    public class DimLightCommandHandler(ILogger<DimLightCommandHandler> logger, DobissClientFactory dobissClientFactory, LightCacheService lightCacheService, IPublishBus publishBus) : ICommandHandler<DimLightCommand>
    {
        public async ValueTask<Unit> Handle(DimLightCommand command, CancellationToken cancellationToken)
        {
            DobissService service = dobissClientFactory.Get()
                ?? throw new ArgumentException("Dobiss client is null");
            Light light = command.Light;

            if (light.ModuleType != ModuleType.DIMMER)
                throw new ArgumentException($"Light {light.Name} is not a dimmable light");

            if (light.CurrentValue != command.NewState)
            {
                await service.DimOutput(light.ModuleKey, light.Key, command.NewState, cancellationToken);
                light.CurrentValue = command.NewState;
                lightCacheService.Update(light);
            }
            else
                logger.LogInformation("Light already in state {State}", command.NewState);
            await publishBus.Publish(new LightChangedMessage(command.NewState.ToString()), $"{BackgroundWorker.topicPath}{light.ModuleKey}x{light.Key}/state", null, cancellationToken);
            return Unit.Value;
        }
    }
}
