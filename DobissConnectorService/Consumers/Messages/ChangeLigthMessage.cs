namespace DobissConnectorService.Consumers.Messages
{
    public class ChangeLigthMessage
    {
        public string State { get; set; } = string.Empty;
        public int? Brightness { get; set; } = null;
    }
}
