namespace TaskMonitoringApp.Cache.Services
{
    public interface IGenericCacheService<T> where T : class
    {
        Task<List<T>> GetCachedDataAsync(string cacheKey, Func<Task<List<T>>> fetchFromDatabase);
        void RemoveFromCache(string cacheKey);
        void UpdateCache(string cacheKey, List<T> updatedData);
    }

}
