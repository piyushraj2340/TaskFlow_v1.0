using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetCategoriesByUserAsync(string userId);
        Task<Category> GetCategoryByIdAsync(int categoryId);
        Task<List<Item>> GetUserItemsByCategoryIdAsync(int categoryId, string userId);
        Task<Category> GetOrCreateCategoryByNameAsync(string name);
    }
}
