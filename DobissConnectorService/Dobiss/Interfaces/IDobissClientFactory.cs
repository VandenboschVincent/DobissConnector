using Microsoft.Extensions.Logging;

namespace DobissConnectorService.Dobiss.Interfaces
{
    public interface IDobissClientFactory
    {
        DobissService Create(string ip, int port, ILogger logger, ILightCacheService lightCacheService);
        DobissService? Get();
    }
}
