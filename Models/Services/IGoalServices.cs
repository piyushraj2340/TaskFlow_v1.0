using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.ViewModel;

namespace TaskMonitoringApp.Models.Services
{
    public interface IGoalServices
    {
        Task AddNewGoal(string UserId, GoalDTO goals);

        Task DeleteGoal(string UserId, int Id);

        Task UpdateGoal(string UserId, GoalDTO goals);

        Task UpdateGoalStatus(string userId, int goalId, Status statusToUpdate);

        Task<GoalDTO> GetGoalById(string UserId, int Id);

        Task<GoalDTOWithTaskNameListDTO> GetAllTaskNameWithStatusAndGoal(string userId, int goalId, Status goalStatus);

        Task<GoalDTOWithTaskListDTO> GetAllTaskWithStatusAndGoal(string userId, int goalId, Status goalStatus);

        Task<IEnumerable<GoalDTO>> GetAllGoals(string UserId, Status status);

        Task<int> GetGoalCountByGoalStatus(string UserId, Status status);

        Task<int> GetGoalCountByGoalStatusAndDateTimeRange(string UserId, Status status, DateTime from, DateTime end);

        Task<GoalProductivityDTO> GetGoalProductivity(string userId);

        Task<IEnumerable<GoalNameDTO>> GetGoalNameBySearchQuery(string userId, string searchQuery);

        Task<GoalNameDTO> GetGoalNameById(string userId, int goalId);
    }
}
