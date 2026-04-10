using HomeAssistantDiscoveryNet;
using Mediator;
using Microsoft.Extensions.Logging;
using MQTTnet;
using ToMqttNet;

namespace DobissConnectorService.CommandHandlers
{
    public class PublishDiscoveryDocumentCommand(string id, string name) : ICommand<bool>
    {
        public string Id { get; set; } = id;
        public string Name { get; set; } = name;
    }
    public class PublishDiscoveryDocument(ILogger<PublishDiscoveryDocument> logger, IMqttConnectionService mqttClient) : ICommandHandler<PublishDiscoveryDocumentCommand, bool>
    {
        public async ValueTask<bool> Handle(PublishDiscoveryDocumentCommand command, CancellationToken cancellationToken)
        {
            var cfg = new MqttBinarySensorDiscoveryConfig
            {
                Name = command.Name,
                ValueTemplate = "{{ value_json.state }}",
                StateTopic = $"{mqttClient.MqttOptions.NodeId}/{command.Name}/ringing",
                UniqueId = $"fermax_{command.Id.Replace(".","_")}_{command.Name}".ToLower(),
                OffDelay = 60,
                Icon = "mdi:doorbell-video"
            };
            var json = cfg.ToJson();

            logger.LogDebug("Publishing discovery document: {Json}", json);
            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"homeassistant/binary_sensor/{command.Name}/config")
                .WithPayload(json)
                .WithRetainFlag(true)
                .Build());
            return true;
        }
    }
}
