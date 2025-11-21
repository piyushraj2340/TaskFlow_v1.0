using TaskMonitoringApp.Models.DTOs;

namespace TaskMonitoringApp.Models.Services
{
    public interface IItemService
    {
        Task<IEnumerable<ItemDto>> GetCollectionItemsAsync(int collectionId, string userId);
        Task<ItemDto> GetItemByIdAsync(int itemId, int collectionId, string userId);
        Task<(ItemDto, string)> CreateItemAsync(int collectionId, CreateItemDto createDto, string userId);
        Task<(bool, string)> UpdateItemAsync(int itemId, int collectionId, UpdateItemDto updateDto, string userId);
        Task<(bool, string)> DeleteItemAsync(int itemId, int collectionId, string userId);
    }
}
