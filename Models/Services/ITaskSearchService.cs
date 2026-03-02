using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;

namespace TaskMonitoringApp.Models.Services
{
    public interface ITaskSearchService
    {
        // This is your existing method
        Task<IEnumerable<TaskDTO>> SearchTasksExcludingRunningTodos(string userId, string query, Status status);

        // This is the new method that includes goal lists
        Task<IEnumerable<TaskDTOWithGoalNameListDTO>> SearchTasksWithGoalsExcludingRunningTodosAsync(string userId, string query, Status status);
    }
}