using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetCategoriesByUserAsync(string userId)
        {
            // Select all categories that are linked to items owned by the specified user
            return await _context.Categories
                .Where(c => c.Items.Any(i => i.Collection.UserId == userId))
                .Distinct()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories.FindAsync(categoryId);
        }

        public async Task<List<Item>> GetUserItemsByCategoryIdAsync(int categoryId, string userId)
        {
            return await _context.Items
                .Include(i => i.Categories)
                .Where(i => i.Collection.UserId == userId && i.Categories.Any(c => c.CategoryId == categoryId))
                .ToListAsync();
        }

        public async Task<Category> GetOrCreateCategoryByNameAsync(string name)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
            if (category == null)
            {
                category = new Category { Name = name };
                await _context.Categories.AddAsync(category);
                // Note: SaveChanges will be called in the service layer
            }
            return category;
        }
    }
}
