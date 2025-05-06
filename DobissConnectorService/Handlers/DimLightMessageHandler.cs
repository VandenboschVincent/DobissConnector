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
    public class DimLightMessageHandler(ILogger<DimLightMessageHandler> logger, DobissClientFactory dobissClientFactory, LightCacheService lightCacheService, IPublishBus publishBus) : ICommandHandler<DimLightMessage>
    {
        public async ValueTask<Unit> Handle(DimLightMessage command, CancellationToken cancellationToken)
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
            await publishBus.Publish(new LightStateMessage(command.NewState.ToString()), $"{BackgroundWorker.topicPath}{light.ModuleKey}x{light.Key}/state", null, cancellationToken);
            return Unit.Value;
        }
    }
}
