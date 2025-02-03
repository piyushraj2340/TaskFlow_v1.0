using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Services
{
    public interface ITaskServices
    {
        Task AddNewTask(string UserId, TaskDTO task, string goalIds); 

        Task UpdateTask(string UserId, TaskDTO task);

        Task UpdateTaskStatus(string userId, int taskId, Status statusToChange);

        Task DeleteTask(string UserId, int Id); 

        Task<IEnumerable<TaskDTO>> GetAllTasks(string UserId, Status status); 

        Task<TaskDTO> GetTaskById(string UserId, int Id); 

        Task<IEnumerable<TaskDTO>> GetAllTasksWithStatusByGoalId(string userId, int goalId, Status taskStatus);

        Task<TaskDTOWithGoalDTOs> GetAllGoalsWithStatusAndTask(string userId, int taskId, Status goalStatus);

        Task<TaskDTOWithGoalNameDTOs> GetAllGoalNamesWithStatusAndTask(string userId, int taskId, Status goalStatus);

        Task<TaskProductivityDTO> GetTaskProductivity(string UserId); 
    }
}
