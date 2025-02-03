using Microsoft.Extensions.Caching.Memory;
using TaskMonitoringApp.Cache.Services;

namespace TaskMonitoringApp.Cache.Business
{
    public class GenericCacheService<T> : IGenericCacheService<T> where T : class
    {
        private readonly IMemoryCache _memoryCache;

        public GenericCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<List<T>> GetCachedDataAsync(string cacheKey, Func<Task<List<T>>> fetchFromDatabase)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out List<T> cachedData))
            {
                cachedData = await fetchFromDatabase();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };

                _memoryCache.Set(cacheKey, cachedData, cacheOptions);
            }

            return cachedData;
        }

        public void RemoveFromCache(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
        }

        public void UpdateCache(string cacheKey, List<T> updatedData)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            _memoryCache.Set(cacheKey, updatedData, cacheOptions);
        }
    }

}
