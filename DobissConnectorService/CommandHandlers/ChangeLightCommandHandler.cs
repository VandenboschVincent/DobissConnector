﻿using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss.Models;
using Mediator;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using DobissConnectorService.CommandHandlers.Commands;
using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss;

namespace DobissConnectorService.CommandHandlers
{
    public class ChangeLightCommandHandler(ILogger<ChangeLightCommandHandler> logger, IDobissClientFactory dobissClientFactory, LightCacheService lightCacheService, IPublishBus publishBus) : ICommandHandler<ChangeLightCommand>
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

            if (light.ModuleType != ModuleType.DIMMER && command.NewState > 0 && command.NewState < 100)
                throw new ArgumentException($"Light {light.Name} is not a dimmable light");

            if (light.CurrentValue != command.NewState)
            {
                await using (await service.DobissClient.Connect(cancellationToken))
                {
                    await service.DimOutput(light.ModuleKey, light.Key, command.NewState.Value, cancellationToken);
                }
                light.CurrentValue = command.NewState.Value;
                lightCacheService.Update(light);
                logger.LogInformation("Light {LightName} set to {State}", light.Name, command.NewState);
            }
            else
                logger.LogInformation("Light {LightName} already in state {State}", light.Name, command.NewState);
            await publishBus.Publish(new LightChangedMessage(command.NewState == 0 ? "OFF" : "ON", command.NewState), $"{BackgroundWorker.topicPath}{light.ModuleKey}x{light.Key}/state", null, cancellationToken);
            return Unit.Value;
        }

    }
}
