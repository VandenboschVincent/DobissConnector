namespace DobissConnectorService.Dobiss.Interfaces
{
    public interface IDobissClient
    {
        ValueTask<IAsyncDisposable> Connect(CancellationToken cancellationToken);
        Task<byte[]> SendRequest(byte[] data, int responseSize, CancellationToken cancellationToken);
    }
}
