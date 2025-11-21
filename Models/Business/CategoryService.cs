using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Models.Business
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly ICollectionRepository _collectionRepo; // To save changes

        public CategoryService(ICategoryRepository categoryRepo, ICollectionRepository collectionRepo)
        {
            _categoryRepo = categoryRepo;
            _collectionRepo = collectionRepo;
        }

        public async Task<IEnumerable<CategoryDto>> GetUserCategoriesAsync(string userId)
        {
            var categories = await _categoryRepo.GetCategoriesByUserAsync(userId);
            return categories.Select(c => new CategoryDto { Id = c.CategoryId, Name = c.Name });
        }

        public async Task<(bool, string)> UpdateUserCategoryAsync(int categoryId, UpdateCategoryDto updateDto, string userId)
        {
            var oldCategory = await _categoryRepo.GetCategoryByIdAsync(categoryId);
            if (oldCategory == null)
            {
                return (false, "Category not found.");
            }

            // Get or create the category with the new name.
            var newCategory = await _categoryRepo.GetOrCreateCategoryByNameAsync(updateDto.Name);

            // If they are the same category, no changes are needed.
            if (oldCategory.CategoryId == newCategory.CategoryId)
            {
                return (true, null);
            }

            // Find all items for this user that use the old category.
            var userItemsWithOldCategory = await _categoryRepo.GetUserItemsByCategoryIdAsync(categoryId, userId);

            if (!userItemsWithOldCategory.Any())
            {
                return (false, "This category is not associated with any of your items.");
            }

            // Re-link items from the old category to the new one.
            foreach (var item in userItemsWithOldCategory)
            {
                item.Categories.Remove(oldCategory);
                if (!item.Categories.Contains(newCategory))
                {
                    item.Categories.Add(newCategory);
                }
            }

            await _collectionRepo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool, string)> DeleteUserCategoryAsync(int categoryId, string userId)
        {
            var categoryToRemove = await _categoryRepo.GetCategoryByIdAsync(categoryId);
            if (categoryToRemove == null)
            {
                return (false, "Category not found.");
            }

            var userItemsWithCategory = await _categoryRepo.GetUserItemsByCategoryIdAsync(categoryId, userId);

            if (!userItemsWithCategory.Any())
            {
                return (false, "This category is not associated with any of your items.");
            }

            // Remove the category from all associated items for this user.
            foreach (var item in userItemsWithCategory)
            {
                item.Categories.Remove(categoryToRemove);
            }

            await _collectionRepo.SaveChangesAsync();
            return (true, null);
        }
    }
}
