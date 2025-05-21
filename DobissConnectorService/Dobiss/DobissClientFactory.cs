using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Services;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace DobissConnectorService.Dobiss
{
    public class DobissClientFactory : IDobissClientFactory
    {
        private DobissService? _instance;

        public DobissService Create(string ip, int port, ILogger logger, LightCacheService lightCacheService)
        {
            return _instance ??= CreateInstance(ip, port, logger, lightCacheService);
        }

        private DobissService CreateInstance(string ip, int port, ILogger logger, LightCacheService lightCacheService)
        {
            DobissClient client = new(ip, port, logger);
            DobissService service = new(client, lightCacheService);
            _instance = service;
            return _instance;
        }

        public DobissService? Get()
        {
            return _instance;
        }
    }
}
