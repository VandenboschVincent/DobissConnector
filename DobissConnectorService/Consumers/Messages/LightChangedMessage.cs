namespace DobissConnectorService.Consumers.Messages
{
    public class LightChangedMessage(string state, int? brightness)
    {
        public string State { get; } = state;
        public int? Brightness { get; set; } = brightness;
    }
}
