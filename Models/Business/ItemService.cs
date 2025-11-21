using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Models.Business
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepo;
        private readonly ICollectionRepository _collectionRepo; // To verify ownership

        public ItemService(IItemRepository itemRepo, ICollectionRepository collectionRepo)
        {
            _itemRepo = itemRepo;
            _collectionRepo = collectionRepo;
        }

        private ItemDto MapItemToDto(Item item)
        {
            return new ItemDto
            {
                ItemId = item.ItemId,
                Name = item.Name,
                Description = item.Description,
                Categories = item.Categories.Select(c => new CategoryTagDto { Id = c.CategoryId, Name = c.Name }).ToList(),
                Tags = item.Tags.Select(t => new CategoryTagDto { Id = t.TagId, Name = t.Name }).ToList()
            };
        }

        public async Task<IEnumerable<ItemDto>> GetCollectionItemsAsync(int collectionId, string userId)
        {
            if (!await _collectionRepo.CollectionExistsAsync(collectionId, userId))
            {
                return null; // Or throw an exception
            }

            var items = await _itemRepo.GetItemsByCollectionIdAsync(collectionId);
            return items.Select(MapItemToDto);
        }

        public async Task<ItemDto> GetItemByIdAsync(int itemId, int collectionId, string userId)
        {
            if (!await _collectionRepo.CollectionExistsAsync(collectionId, userId))
            {
                return null;
            }

            var item = await _itemRepo.GetItemByIdAsync(itemId, collectionId);
            return item == null ? null : MapItemToDto(item);
        }

        public async Task<(ItemDto, string)> CreateItemAsync(int collectionId, CreateItemDto createDto, string userId)
        {
            if (!await _collectionRepo.CollectionExistsAsync(collectionId, userId))
            {
                return (null, "Collection not found.");
            }

            if (await _itemRepo.ItemNameExistsInCollectionAsync(createDto.Name, collectionId))
            {
                return (null, "An item with this name already exists in this collection.");
            }

            var newItem = new Item
            {
                Name = createDto.Name,
                Description = createDto.Description,
                CollectionId = collectionId
            };

            foreach (var catName in createDto.Categories)
            {
                newItem.Categories.Add(await _itemRepo.GetOrCreateCategoryAsync(catName));
            }
            foreach (var tagName in createDto.Tags)
            {
                newItem.Tags.Add(await _itemRepo.GetOrCreateTagAsync(tagName));
            }

            await _itemRepo.AddItemAsync(newItem);
            await _collectionRepo.SaveChangesAsync(); // Use one of the repos to save

            return (MapItemToDto(newItem), null);
        }

        public async Task<(bool, string)> UpdateItemAsync(int itemId, int collectionId, UpdateItemDto updateDto, string userId)
        {
            if (!await _collectionRepo.CollectionExistsAsync(collectionId, userId))
            {
                return (false, "Collection not found.");
            }

            var item = await _itemRepo.GetItemByIdAsync(itemId, collectionId);
            if (item == null)
            {
                return (false, "Item not found.");
            }

            if (item.Name.ToLower() != updateDto.Name.ToLower() && await _itemRepo.ItemNameExistsInCollectionAsync(updateDto.Name, collectionId))
            {
                return (false, "An item with this name already exists in this collection.");
            }

            item.Name = updateDto.Name;
            item.Description = updateDto.Description;

            // Update Categories and Tags
            item.Categories.Clear();
            item.Tags.Clear();
            foreach (var catName in updateDto.Categories)
            {
                item.Categories.Add(await _itemRepo.GetOrCreateCategoryAsync(catName));
            }
            foreach (var tagName in updateDto.Tags)
            {
                item.Tags.Add(await _itemRepo.GetOrCreateTagAsync(tagName));
            }

            await _collectionRepo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool, string)> DeleteItemAsync(int itemId, int collectionId, string userId)
        {
            if (!await _collectionRepo.CollectionExistsAsync(collectionId, userId))
            {
                return (false, "Collection not found.");
            }

            var item = await _itemRepo.GetItemByIdAsync(itemId, collectionId);
            if (item == null)
            {
                return (false, "Item not found.");
            }

            _itemRepo.DeleteItem(item);
            await _collectionRepo.SaveChangesAsync();
            return (true, null);
        }
    }
}
