using DobissConnectorService.Dobiss.Models;

namespace DobissConnectorService.Dobiss.Interfaces
{
    public interface ILightCacheService
    {
        Task<IEnumerable<Light>> GetAll();
        Task<Light?> Get(int module, int key);
        Task Add(Light light);
        Task Update(Light light);
    }
}
