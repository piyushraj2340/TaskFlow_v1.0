using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface ITodoRepository
    {
        Task<IEnumerable<T>> GetAllTodoAsync<T>(string UserId, Status status, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllTodoAsync<T>(string userId, Status status, DateTime selectDate, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllTodoAsync<T>(string UserId, int taskId, Status status, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllTodoAsync<T>(string userId, int taskId, Status status, DateTime selectDate, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllTodoWithStatusByGoalId<T>(string userId, int goalId, Status todoStatus, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllTodoWithStatusByTaskId<T>(string userId, int taskId, Status todoStatus, ResponseDataMode mode) where T : class;

        Task<T> GetTodoByIdAsync<T>(string UserId, int Id, ResponseDataMode mode) where T : class;

        Task<T> GetTodoProgressAnalysesAsync<T>(string userId, DateTime forDate, ResponseDataMode mode) where T : class;
        Task<TodoProgressAnalysisDTO> GetTodoProgressAnalysesAsync(string userId, int taskId);

        Task<T> GetTodoProgressAnalysesAsync<T>(string userId, ResponseDataMode mode) where T : class;

        Task AddTodoAsync(string UserId, Todo todoList);

        Task UpdateTodoAsync(string UserId, Todo todoList);

        Task UpdateTodoStatusAsync(string userId, int todoId, Status statusToChange);

        Task DeleteTodoAsync(string UserId, int Id);

        Task UpdateTodoNotesAsync(string userId, int todoId, string notes);

        Task AddBulkTodosAsync(BulkTodoCreateDTO bulkDto);

        Task<IEnumerable<Tasks>> GetCandidateTasksForTodoAsync(string userId);

        Task<IEnumerable<int>> GetExistingTodoTaskIdsAsync(string userId, DateTime today, DateTime tomorrow);

        Task AddTodosBulkAsync(IEnumerable<Todo> todos);
    }
}
