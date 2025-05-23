﻿using DobissConnectorService.CommandHandlers.Commands;
using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss.Models;
using Mediator;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using System.Text.RegularExpressions;

namespace DobissConnectorService.Consumers
{
    public partial class LightChangedConsumer(ILogger<LightChangedConsumer> logger, LightCacheService lightCacheService, IMediator mediator) : IConsumer<IConsumerContext<ChangeLigthMessage>>
    {
        public async Task OnHandle(IConsumerContext<ChangeLigthMessage> message, CancellationToken cancellationToken)
        {
            logger.LogDebug("Handle message {@ChangeLigthMessage}", message);
            string? path = message.Headers["origPath"].ToString();
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path is null or empty");
            }
            var matches = MatchGroupRegex().Match(path);
            int module = int.Parse(matches.Groups[1].ValueSpan);
            int device = int.Parse(matches.Groups[2].ValueSpan);

            Light? light = lightCacheService.Get(module, device)
                ?? throw new ArgumentException($"Light with module {module} and device {device} not found");
            await mediator.Send(new ChangeLightCommand(light, message.Message.Brightness ?? StateToInt(message.Message.State)), cancellationToken);
        }

        private static int StateToInt(string state)
        {
            switch(state.ToLower())
            {
                case "on":
                case "true":
                case "1":
                    return 100;
                case "off":
                case "false":
                    return 0;
            }
            if (int.TryParse(state, out int result))
            {
                return result;
            }
            throw new ArgumentException($"Invalid state: {state}", nameof(state));
        }

        [GeneratedRegex("(\\d+)[xX](\\d+)")]
        private static partial Regex MatchGroupRegex();
    }
}
