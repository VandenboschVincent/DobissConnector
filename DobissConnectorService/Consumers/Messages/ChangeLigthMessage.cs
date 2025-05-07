namespace DobissConnectorService.Consumers.Messages
{
    public class ChangeLigthMessage
    {
        public string State { get; set; } = string.Empty;
        public int? brightness { get; set; } = null;
    }
}
