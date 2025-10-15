

using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface ICollectionRepository
    {
        Task<IEnumerable<Collection>> GetCollectionsByUserIdAsync(string userId);
        Task<Collection> GetCollectionByIdAsync(int collectionId, string userId);
        Task<bool> CollectionExistsAsync(int collectionId, string userId);
        Task<bool> CollectionNameExistsForUserAsync(string name, string userId);
        Task AddCollectionAsync(Collection collection);
        void UpdateCollection(Collection collection); // Not async as it only changes state
        void DeleteCollection(Collection collection); // Not async as it only changes state
        Task<bool> SaveChangesAsync();
    }
}
