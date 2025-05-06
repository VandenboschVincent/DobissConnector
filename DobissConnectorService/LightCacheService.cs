using DobissConnectorService.Dobiss.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DobissConnectorService
{
    public class LightCacheService
    {
        private readonly IMemoryCache _cache;
        private const string CacheKey = "LightCache";

        public LightCacheService(IMemoryCache cache)
        {
            _cache = cache;
            // Initialize the cache with an empty dictionary if not already present
            if (!_cache.TryGetValue(CacheKey, out Dictionary<string, Light>? _))
            {
                _cache.Set(CacheKey, new Dictionary<string, Light>());
            }
        }

        private static string GetKey(int module, int key)
        {
            return $"{module}x{key}";
        }

        public IEnumerable<Light> GetAll()
        {
            var lights = _cache.Get<Dictionary<string, Light>>(CacheKey);
            return lights?.Values ?? Enumerable.Empty<Light>();
        }

        public Light? Get(int module, int key)
        {
            var lights = _cache.Get<Dictionary<string, Light>>(CacheKey);
            return lights != null && lights.TryGetValue(GetKey(module, key), out var light) ? light : null;
        }

        public void Add(Light light)
        {
            var lights = _cache.Get<Dictionary<string, Light>>(CacheKey);
            string key = GetKey(light.ModuleKey, light.Key);
            if (lights != null && !lights.ContainsKey(key))
            {
                lights[key] = light;
                _cache.Set(CacheKey, lights); // Update the cache
            }
        }

        public void Update(Light light)
        {
            var lights = _cache.Get<Dictionary<string, Light>>(CacheKey);
            string key = GetKey(light.ModuleKey, light.Key);
            if (lights != null && lights.ContainsKey(key))
            {
                lights[key] = light;
                _cache.Set(CacheKey, lights); // Update the cache
            }
        }
    }
}
