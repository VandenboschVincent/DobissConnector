namespace DobissConnectorService.Consumers.Messages
{
    public class LightStateMessage(string state)
    {
        public string State { get; } = state;
    }
}
