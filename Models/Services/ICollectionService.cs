using TaskMonitoringApp.Models.DTOs;

namespace TaskMonitoringApp.Models.Services
{
    public interface ICollectionService
    {
        Task<IEnumerable<CollectionDto>> GetUserCollectionsAsync(string userId);
        Task<CollectionDto> GetUserCollectionByIdAsync(int collectionId, string userId);
        Task<(CollectionDto, string)> CreateCollectionAsync(CreateCollectionDto createDto, string userId);
        Task<(bool, string)> UpdateCollectionAsync(int collectionId, UpdateCollectionDto updateDto, string userId);
        Task<(bool, string)> DeleteCollectionAsync(int collectionId, string userId);
    }
}
