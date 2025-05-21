using System.Net.Sockets;

namespace DobissConnectorService.Dobiss.Utils
{
    public class MySocket(string ip, int port) : Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), IAsyncDisposable
    {
        private SemaphoreSlim? semaphoreSlim;
        public async Task Connect(SemaphoreSlim semaphoreSlim, CancellationToken cancellationToken)
        {
            this.semaphoreSlim = semaphoreSlim;
            await semaphoreSlim.WaitAsync(cancellationToken);
            await base.ConnectAsync(ip, port, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await base.DisconnectAsync(false);
            base.Dispose();
            semaphoreSlim?.Release();
        }
    }
}
