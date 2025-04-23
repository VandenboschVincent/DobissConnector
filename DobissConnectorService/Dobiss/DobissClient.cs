using DobissConnectorService.Dobiss.Utils;
using System.Net.Http;
using System.Net.Sockets;

namespace DobissConnectorService.Dobiss
{
    public class DobissClient : IDisposable
    {
        private const int DelayBeforeNextAttempt = 25;
        private const int NumberOfAttempts = 25;
        private const int DefaultMaxLines = 100;
        private const int SocketTimeout = 3000;

        private readonly string ip;
        private readonly int port;
        private readonly ILogger logger;
        private TcpClient tcpClient = new();
        private bool keepConnectionOpen = false;

        public DobissClient(string ip, int port, ILogger logger)
        {
            this.ip = ip;
            this.port = port;
            this.logger = logger;
            tcpClient.ReceiveTimeout = SocketTimeout;
            tcpClient.SendTimeout = SocketTimeout;
        }

        public async Task<byte[]> SendRequest<T>(IDobissRequest<T> request, CancellationToken cancellationToken) where T : class
        {
            int maxLines = request.GetMaxOutputLines() == 0 ? DefaultMaxLines : request.GetMaxOutputLines();
            return await SendRequest(request.GetRequestBytes(), maxLines, cancellationToken);
        }

        public async Task<byte[]> SendRequest(byte[] parameters, CancellationToken cancellationToken)
        {
            return await SendRequest(parameters, DefaultMaxLines, cancellationToken);
        }

        public async Task<byte[]> SendRequest(byte[] parameters, int maxLines, CancellationToken cancellationToken)
        {
            try
            {
                if (!tcpClient.Connected)
                {
                    logger.LogDebug("Opening new socket");
                    tcpClient = new();
                    await tcpClient.ConnectAsync(ip, port, cancellationToken);
                }

                NetworkStream stream = tcpClient.GetStream();
                // Send request
                await stream.WriteAsync(parameters, cancellationToken);
                logger.LogDebug("Socket Request: {Parameters}", ConversionUtils.BytesToHex(parameters));

                byte[] data = new byte[maxLines * 16];
                int count = 0;

                while (count < maxLines && !cancellationToken.IsCancellationRequested)
                {
                    int attempt = 0;
                    while (!stream.DataAvailable && attempt < NumberOfAttempts)
                    {
                        await Task.Delay(DelayBeforeNextAttempt, cancellationToken);
                        attempt++;
                    }

                    if (stream.DataAvailable)
                    {
                        byte[] temp = new byte[16];
                        int bytesRead = await stream.ReadAsync(temp, cancellationToken);
                        if (bytesRead > 0)
                        {
                            logger.LogDebug("Socket Response: {Response}", ConversionUtils.BytesToHex(temp));
                            Array.Copy(temp, 0, data, count * 16, temp.Length);
                        }
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                    return [];

                if (count == 0)
                {
                    string hex = ConversionUtils.BytesToHex(parameters);
                    logger.LogError("No response for request {Hex}", hex);
                    throw new ArgumentNullException($"No response for request {hex}");
                }

                // Return from offset 32 to count*16
                int length = count * 16;
                if (length <= 32) return [];
                int resultLength = length - 32;
                byte[] result = new byte[resultLength];
                Array.Copy(data, 32, result, 0, resultLength);
                return result;
            }
            catch (SocketException socketEx) when (socketEx.SocketErrorCode == SocketError.TimedOut)
            {
                logger.LogError(socketEx, "Timeout for request {Parameters}", ConversionUtils.BytesToHex(parameters));
                throw;
            }
            catch (TaskCanceledException)
            {
                return [];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Connection error occurred.");
                throw;
            }
            finally
            {
                if (tcpClient.Connected && !keepConnectionOpen)
                {
                    tcpClient.Close();
                }
            }
        }

        public void SetKeepConnectionOpen(bool keepOpen)
        {
            this.keepConnectionOpen = keepOpen;
        }

        public void CloseConnection()
        {
            if (tcpClient.Connected)
            {
                tcpClient.Close();
            }
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}