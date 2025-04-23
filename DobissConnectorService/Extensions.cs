using SlimMessageBus.Host.Mqtt;
using SlimMessageBus.Host;
using MQTTnet;
using System.Text.RegularExpressions;
using DobissConnectorService.Consumers;

namespace DobissConnectorService
{
    public static class Extensions
    {
        public static MessageBusBuilder WithCustomProviderMqtt(this MessageBusBuilder mbb, Action<MqttMessageBusSettings> configure)
        {
            if (mbb == null) throw new ArgumentNullException(nameof(mbb));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var providerSettings = new MqttMessageBusSettings();
            configure(providerSettings);

            return mbb.WithProvider(settings => new CustomMqttMessageBus(settings, providerSettings));
        }

        public static Action<object, MqttApplicationMessage> GetMessageModifier(this HasProviderExtensions producerSettings)
        {
            return producerSettings.GetOrDefault<Action<object, MqttApplicationMessage>>(nameof(SetMessageModifier), null);
        }

        public static HasProviderExtensions SetMessageModifier(this HasProviderExtensions producerSettings, Action<object, MqttApplicationMessage> messageModifierAction)
        {
            producerSettings.Properties[nameof(SetMessageModifier)] = messageModifierAction;
            return producerSettings;
        }

        public static bool CheckTopic(string allowedTopic, string topic)
        {
            var realTopicRegex = allowedTopic.Replace(@"/", @"\/").Replace("+", @"[a-zA-Z0-9 _.-]*").Replace("#", @"[a-zA-Z0-9 \/_#+.-]*");
            var regex = new Regex(realTopicRegex);
            return regex.IsMatch(topic);
        }
    }
}
