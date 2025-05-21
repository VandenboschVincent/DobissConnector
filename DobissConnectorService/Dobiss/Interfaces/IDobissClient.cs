namespace DobissConnectorService.Dobiss.Interfaces
{
    public interface IDobissClient
    {
        Task<byte[]> SendRequest(byte[] parameters, int maxLines = 100, CancellationToken cancellationToken = default);
    }
}
