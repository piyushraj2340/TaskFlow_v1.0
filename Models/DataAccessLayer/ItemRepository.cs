using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class ItemRepository : IItemRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Item>> GetItemsByCollectionIdAsync(int collectionId)
        {
            return await _context.Items
                .Include(i => i.Categories)
                .Include(i => i.Tags)
                .Where(i => i.CollectionId == collectionId)
                .ToListAsync();
        }

        public async Task<Item> GetItemByIdAsync(int itemId, int collectionId)
        {
            return await _context.Items
                .Include(i => i.Categories)
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(i => i.ItemId == itemId && i.CollectionId == collectionId);
        }

        public async Task<bool> ItemNameExistsInCollectionAsync(string name, int collectionId)
        {
            return await _context.Items.AnyAsync(i => i.CollectionId == collectionId && i.Name.ToLower() == name.ToLower());
        }

        public async Task AddItemAsync(Item item)
        {
            await _context.Items.AddAsync(item);
        }

        public void DeleteItem(Item item)
        {
            _context.Items.Remove(item);
        }

        public async Task<Category> GetOrCreateCategoryAsync(string name)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
            if (category == null)
            {
                category = new Category { Name = name };
                await _context.Categories.AddAsync(category);
            }
            return category;
        }

        public async Task<Tag> GetOrCreateTagAsync(string name)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
            if (tag == null)
            {
                tag = new Tag { Name = name };
                await _context.Tags.AddAsync(tag);
            }
            return tag;
        }
    }
}
