using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface ITodoRepository
    {
        Task<IEnumerable<T>> GetAllTodoAsync<T>(string UserId, Status status, ResponseDataMode mode) where T: class;

        Task<IEnumerable<T>> GetAllTodoWithStatusByGoalId<T>(string userId, int goalId, Status todoStatus, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllTodoWithStatusByTaskId<T>(string userId, int goalId, Status todoStatus, ResponseDataMode mode) where T : class;

        Task<T> GetTodoByIdAsync<T>(string UserId, int Id, ResponseDataMode mode) where T: class;

        Task<T> GetTodoProgressForTodaysAsync<T>(string userId, ResponseDataMode mode) where T: class;

        Task AddTodoAsync(string UserId, Todo todoList);

        Task UpdateTodoAsync(string UserId, Todo todoList);

        Task UpdateTodoStatusAsync(string userId, int todoId, Status statusToChange);

        Task DeleteTodoAsync(string UserId, int Id);
    }
}
