
using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Services;
using Microsoft.Extensions.Logging;

namespace DobissConnectorService.Dobiss
{
    public class DobissClientFactory
    {
        private DobissService? _instance;

        public DobissService Create(string ip, int port, Dictionary<int, ModuleType> moduleTypeMap, ILogger logger, LightCacheService lightCacheService)
        {
            return _instance ??= new DobissService(new DobissClient(ip, port, logger), moduleTypeMap, lightCacheService);
        }

        public DobissService? Get()
        {
            return _instance;
        }
    }
}
