using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Services;
using Microsoft.Extensions.Logging;

namespace DobissConnectorService.Dobiss.Interfaces
{
    public interface IDobissClientFactory
    {
        DobissService Create(string ip, int port, ILogger logger, LightCacheService lightCacheService);
        DobissService? Get();
    }
}
