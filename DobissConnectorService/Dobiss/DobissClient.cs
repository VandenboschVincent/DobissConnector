using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Utils;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace DobissConnectorService.Dobiss
{
    public class DobissClient(string host, int port, ILogger logger) : IDobissClient
    {
        private const int SocketTimeout = 5000;
        private const int RECV_SIZE = 1024;
        private MySocket? _socket;
        private readonly SemaphoreSlim semaphoreSlim = new(1);

        public async ValueTask<IAsyncDisposable> Connect(CancellationToken cancellationToken)
        {
            await semaphoreSlim.WaitAsync(cancellationToken);
            _socket = new MySocket(host, port)
            {
                SendTimeout = SocketTimeout,
                ReceiveTimeout = SocketTimeout
            };
            await _socket.Connect(semaphoreSlim, cancellationToken);
            return _socket;
        }

        public async Task<byte[]> SendRequest(byte[] data, int responseSize, CancellationToken cancellationToken)
        {
            try
            {
                if (_socket == null)
                {
                    throw new ArgumentException("socket not connected");
                }
                using CancellationTokenSource timeoutCts = new(SocketTimeout);
                using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
                List<byte> _recvBuffer = [];
                await _socket.SendAsync(data, cancellationTokenSource.Token);
                int totalSize = data.Length + ((32 - (data.Length % 32)) % 32)
                              + responseSize + ((32 - (responseSize % 32)) % 32);

                while (_recvBuffer.Count < totalSize && _socket != null)
                {
                    byte[] buffer = new byte[RECV_SIZE];
                    int received = await _socket.ReceiveAsync(buffer, cancellationTokenSource.Token);
                    if (received > 0)
                        _recvBuffer.AddRange(buffer[..received]);
                    else
                        break;
                }

                if (_recvBuffer.Count < totalSize)
                    return [];

                byte[] response = [.. _recvBuffer
                    .Skip(data.Length + ((32 - (data.Length % 32)) % 32))
                    .Take(responseSize)];

                return response;
            }
            catch (SocketException socketEx) when (socketEx.SocketErrorCode == SocketError.TimedOut)
            {
                logger.LogError(socketEx, "Timeout for request {Parameters}", Convert.ToHexString(data));
            }
            catch (TaskCanceledException timedOutEx)
            {
                logger.LogWarning(timedOutEx, "Timed out");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Connection error occurred.");
            }
            return [];
        }
    }
}