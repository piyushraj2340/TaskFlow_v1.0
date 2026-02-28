using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface IGoalRepository
    {
        Task<IEnumerable<T>> GetAllGoalsAsync<T>(string userId, Status status, ResponseDataMode mode) where T : class;

        Task<T> GetGoalByIdAsync<T>(string userId, int Id, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllGoalsWithStatusByTaskId<T>(string userId, int taskId, Status status, ResponseDataMode mode) where T : class;

        Task AddGoalAsync(string userName, Goals goals);

        Task UpdateGoalAsync(string userId, Goals goals);

        Task UpdateGoalStatusAsync(string userId, int goalId, Status statusToChange);

        Task DeleteGoalAsync(string userId, int Id);

        Task<int> GetGoalCountByGoalStatus(string userId, Status status);

        Task<int> GetGoalCountByGoalStatusAndDateTimeRange(string userId, Status status, DateTime from, DateTime end);

        Task<IEnumerable<GoalNameDTO>> GetGoalNameBySearchQueryAsync(string userId,  string searchQuery);

        // New Methods
        Task<IEnumerable<GoalDTO>> GetRootGoalsAsync(string userId, Status status);
        Task<IEnumerable<GoalDTO>> GetChildGoalsAsync(string userId, int parentId);

        Task UpdateAutoStartedGoalsAsync(string userId);
        Task UpdateEndedGoalsAsync(string userId);
    }
}
