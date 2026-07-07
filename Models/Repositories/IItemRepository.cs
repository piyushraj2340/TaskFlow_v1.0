using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface IItemRepository
    {
        Task<IEnumerable<Item>> GetItemsByCollectionIdAsync(int collectionId);
        Task<Item> GetItemByIdAsync(int itemId, int collectionId);
        Task<bool> ItemNameExistsInCollectionAsync(string name, int collectionId);
        Task AddItemAsync(Item item);
        void DeleteItem(Item item);
        Task<Category> GetOrCreateCategoryAsync(string name);
        Task<Tag> GetOrCreateTagAsync(string name);
    }
}
