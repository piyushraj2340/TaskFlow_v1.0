using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Services
{
    public interface ITaskServices
    {
        Task AddNewTask(string UserId, TaskDTO task, string goalIds); 

        Task UpdateTask(string UserId, TaskDTO task, string goalIds);

        Task UpdateTask(string UserId, TaskDTO task);

        Task UpdateTaskStatus(string userId, int taskId, Status statusToChange);

        Task DeleteTask(string UserId, int Id); 

        Task<IEnumerable<TaskDTO>> GetAllTasks(string UserId, Status status); 

        Task<TaskDTO> GetTaskById(string UserId, int Id); 

        Task<IEnumerable<TaskDTO>> GetAllTasksWithStatusByGoalId(string userId, int goalId, Status taskStatus);

        Task<TaskDTOWithGoalListDTO> GetAllGoalsWithStatusAndTask(string userId, int taskId, Status goalStatus);

        Task<TaskDTOWithGoalNameListDTO> GetAllGoalNamesWithStatusAndTask(string userId, int taskId, Status goalStatus);

        Task<TaskProductivityDTO> GetTaskProductivity(string UserId);

        Task<TaskProductivityDTO> GetTaskProductivity(string UserId, int goalId);
        Task<int> GetTaskCountByTaskStatus(string userId, Status status);
        Task<int> GetTaskCountByTaskStatus(string userId, int goalId, Status status);

        Task<int> GetTaskCountByTaskStatusAndDateTimeRange(string userId, int goalId, Status status, DateTime from, DateTime end);
        Task<int> GetTaskCountByTaskStatusAndDateTimeRange(string userId, Status status, DateTime from, DateTime end);
        Task<IEnumerable<TaskNameDTO>> SearchTasks(string userId, string query);
        Task<IEnumerable<TaskDTOWithGoalNameListDTO>> SearchTasksWithGoals(string userId, string query, Status status);
    }
}
