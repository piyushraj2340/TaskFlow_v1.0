using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Models.Business
{
    public class CollectionService : ICollectionService
    {
        private readonly ICollectionRepository _collectionRepo;

        public CollectionService(ICollectionRepository collectionRepo)
        {
            _collectionRepo = collectionRepo;
        }

        public async Task<IEnumerable<CollectionDto>> GetUserCollectionsAsync(string userId)
        {
            var collections = await _collectionRepo.GetCollectionsByUserIdAsync(userId);
            return collections.Select(c => new CollectionDto
            {
                CollectionId = c.CollectionId,
                Name = c.Name,
                ItemCount = c.Items.Count
            });
        }

        public async Task<CollectionDto> GetUserCollectionByIdAsync(int collectionId, string userId)
        {
            var collection = await _collectionRepo.GetCollectionByIdAsync(collectionId, userId);
            if (collection == null) return null;

            return new CollectionDto
            {
                CollectionId = collection.CollectionId,
                Name = collection.Name,
                ItemCount = collection.Items.Count
            };
        }

        public async Task<(CollectionDto, string)> CreateCollectionAsync(CreateCollectionDto createDto, string userId)
        {
            if (await _collectionRepo.CollectionNameExistsForUserAsync(createDto.Name, userId))
            {
                return (null, "A collection with this name already exists.");
            }

            var collection = new Collection { Name = createDto.Name, UserId = userId };
            await _collectionRepo.AddCollectionAsync(collection);
            await _collectionRepo.SaveChangesAsync();

            var resultDto = new CollectionDto { CollectionId = collection.CollectionId, Name = collection.Name, ItemCount = 0 };
            return (resultDto, null);
        }

        public async Task<(bool, string)> UpdateCollectionAsync(int collectionId, UpdateCollectionDto updateDto, string userId)
        {
            var collection = await _collectionRepo.GetCollectionByIdAsync(collectionId, userId);
            if (collection == null)
            {
                return (false, "Collection not found or you do not have permission to edit it.");
            }

            // Check if the new name is already taken by another collection of the same user
            if (collection.Name.ToLower() != updateDto.Name.ToLower() && await _collectionRepo.CollectionNameExistsForUserAsync(updateDto.Name, userId))
            {
                return (false, "A collection with this name already exists.");
            }

            collection.Name = updateDto.Name;
            _collectionRepo.UpdateCollection(collection);
            await _collectionRepo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool, string)> DeleteCollectionAsync(int collectionId, string userId)
        {
            var collection = await _collectionRepo.GetCollectionByIdAsync(collectionId, userId);
            if (collection == null)
            {
                return (false, "Collection not found or you do not have permission to delete it.");
            }

            _collectionRepo.DeleteCollection(collection);
            await _collectionRepo.SaveChangesAsync();
            return (true, null);
        }
    }
}
