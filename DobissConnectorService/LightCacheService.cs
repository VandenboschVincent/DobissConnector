using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DobissConnectorService
{
    public class LightCacheService : ILightCacheService
    {
        private readonly IMemoryCache _cache;
        private const string CacheKey = "LightCache";

        public LightCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        private static string GetKey(int module, int key)
        {
            return $"{module}x{key}";
        }

        private async Task<Dictionary<string, Light>?> GetOrCreate()
        {
            return await _cache.GetOrCreateAsync(CacheKey, (entry) => Task.FromResult(new Dictionary<string, Light>()));
        }

        public async Task<IEnumerable<Light>> GetAll()
        {
            var lights = await GetOrCreate();
            return lights?.Values ?? Enumerable.Empty<Light>();
        }

        public async Task<Light?> Get(int module, int key)
        {
            var lights = await GetOrCreate();
            return lights != null && lights.TryGetValue(GetKey(module, key), out var light) ? light : null;
        }

        public async Task Add(Light light)
        {
            var lights = await GetOrCreate();
            string key = GetKey(light.ModuleKey, light.Key);
            if (lights != null && !lights.ContainsKey(key))
            {
                lights[key] = light;
                _cache.Set(CacheKey, lights); // Update the cache
            }
        }

        public async Task Update(Light light)
        {
            var lights = await GetOrCreate();
            string key = GetKey(light.ModuleKey, light.Key);
            if (lights != null && lights.ContainsKey(key))
            {
                lights[key] = light;
                _cache.Set(CacheKey, lights); // Update the cache
            }
        }
    }
}
