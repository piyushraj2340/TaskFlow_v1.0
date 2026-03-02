using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface ITaskRepository
    {
        Task<IEnumerable<T>> GetAllTasksAsync<T>(string UserId, Status status, ResponseDataMode mode) where T : class;

        Task<T> GetTasksByIdAsync<T>(string UserId, int Id, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllTasksWithStatusByGoalId<T>(string userId, int goalId, Status taskStatus, ResponseDataMode mode) where T : class;

        Task AddTasksAsync(string UserId, TaskDTO tasks, string goalIds);

        Task UpdateTasksAsync(string UserId, Tasks tasks, string goalIds);

        Task UpdateTasksAsync(string UserId, Tasks tasks);

        Task UpdateTaskStatusAsync(string userId, int taskId, Status statusToChange);

        Task DeleteTasksAsync(string UserId, int Id);

        Task<int> GetTaskCountByTaskStatus(string UserId, Status status);

        Task<int> GetTaskCountByTaskStatusAndDateTimeRange(string UserId, Status status, DateTime from, DateTime end);

        Task<int> GetTaskCountByTaskStatus(string UserId, int goalId, Status status);

        Task<int> GetTaskCountByTaskStatusAndDateTimeRange(string UserId, int goalId, Status status, DateTime from, DateTime end);

        Task<IEnumerable<TaskNameDTO>> SearchTasks(string userId, string query);

        Task<IEnumerable<TaskDTOWithGoalNameListDTO>> SearchTasksWithGoals(string userId, string query, Status status);
        
        Task<int> AddUpdateTaskWithGoalsAsync(string userId, TaskDTO task, IEnumerable<int> goalIds, int mode);

        Task UpdateAutoStartedTasksAsync(string userId);
        Task UpdateEndedTasksAsync(string userId);
    }
}
