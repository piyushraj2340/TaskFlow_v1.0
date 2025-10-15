using TaskMonitoringApp.Models.DTOs;

namespace TaskMonitoringApp.Models.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetUserCategoriesAsync(string userId);
        Task<(bool, string)> UpdateUserCategoryAsync(int categoryId, UpdateCategoryDto updateDto, string userId);
        Task<(bool, string)> DeleteUserCategoryAsync(int categoryId, string userId);
    }
}
