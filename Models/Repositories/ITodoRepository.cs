using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface ITodoRepository
    {
        Task<IEnumerable<Todo>> GetAllTodoAsync(string UserId, Status status);

        Task<IEnumerable<Todo>> GetAllTodoAsync(string UserId);

        Task<Todo> GetTodoByIdAsync(string UserId, int Id);

        Task AddTodoAsync(string UserId, Todo todoList);

        Task UpdateTodoAsync(string UserId, Todo todoList);

        Task DeleteTodoAsync(string UserId, int Id);

        Task<int> GetTodoCountByTodoStatus(string UserId, Status status);

        Task<int> GetTodoCountByTodoStatusAndDateTimeRange(string UserId, Status status, DateTime from, DateTime end);
    }
}
