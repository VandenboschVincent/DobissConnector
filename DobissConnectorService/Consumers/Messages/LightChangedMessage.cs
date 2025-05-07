namespace DobissConnectorService.Consumers.Messages
{
    public class LightChangedMessage(string state)
    {
        public string State { get; } = state;
    }
}
