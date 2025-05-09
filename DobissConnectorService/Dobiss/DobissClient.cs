﻿using DobissConnectorService.Dobiss.Utils;
using Microsoft.Extensions.Logging;
using SuperSimpleTcp;
using System.Net.Sockets;

namespace DobissConnectorService.Dobiss
{
    public class DobissClient(string ip, int port, ILogger logger)
    {
        private const int DefaultMaxLines = 100;
        private const int SocketTimeout = 5000;

        public async Task<byte[]> SendRequest<T>(IDobissRequest<T> request, CancellationToken cancellationToken) where T : class
        {
            int maxLines = request.GetMaxOutputLines() == 0 ? DefaultMaxLines : request.GetMaxOutputLines();
            return await SendRequest(request.GetRequestBytes(), maxLines, cancellationToken);
        }

        public async Task<byte[]> SendRequest(byte[] parameters, CancellationToken cancellationToken)
        {
            return await SendRequest(parameters, DefaultMaxLines, cancellationToken);
        }

        private async Task<byte[]> SendRequest(byte[] parameters, int maxLines, CancellationToken cancellationToken)
        {
            try
            {
                using SimpleTcpClient tcpClient = new(ip, port);
                using CancellationTokenSource timeoutCts = new(SocketTimeout);
                using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
                var buffer = new List<byte>();
                tcpClient.Events.DataReceived += (sender, e) =>
                {
                    buffer.AddRange(e.Data.Slice(0, 32));
                    if (buffer.Count >= maxLines * 16)
                    {
                        cancellationTokenSource.Cancel();
                    }
                };
                tcpClient.Connect();

                logger.LogTrace("Socket Request: {Parameters}", ConversionUtils.BytesToHex(parameters));
                await tcpClient.SendAsync(parameters, cancellationToken);
                await Task.Delay(int.MaxValue, cancellationTokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                if (timeoutCts.Token.IsCancellationRequested)
                {
                    logger.LogInformation("Max time reached for request");
                }

                //Filter out first 32 bytes
                logger.LogTrace("Socket Response: {Response}", ConversionUtils.BytesToHex([.. buffer[32..]]));

                return [.. buffer[32..]];
            }
            catch (SocketException socketEx) when (socketEx.SocketErrorCode == SocketError.TimedOut)
            {
                logger.LogError(socketEx, "Timeout for request {Parameters}", ConversionUtils.BytesToHex(parameters));
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