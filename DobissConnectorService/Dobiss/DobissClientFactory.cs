
using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Services;

namespace DobissConnectorService.Dobiss
{
    public class DobissClientFactory
    {
        private DobissService? _instance;

        public DobissService Create(string ip, int port, Dictionary<int, ModuleType> moduleTypeMap, ILogger logger)
        {
            return _instance ??= new DobissService(new DobissClient(ip, port, logger), moduleTypeMap);
        }

        public DobissService? Get()
        {
            return _instance;
        }
    }
}
