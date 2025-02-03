using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TodoMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Services
{
    public interface ITodoServices
    {

        Task<TodoProgressAnalysis> CalculateProductivityForToday(string UserId);

        Task AddNewTodo (string UserId, Todo todo);

        Task<Todo> GetTodoById(string UserId, int id);

        Task<IEnumerable<Todo>> GetAllTodo(string UserId, Status status);

        Task<IEnumerable<Todo>> GetAllTodo(string UserId); // All todo...

        Task DeleteTodoById(string UserId, int id);

        Task UpdateTodo(string UserId, Todo todo);

        Task MarkAsComplete(string UserId, Todo todo);

        Task MoveToRunning(string UserId, Todo todo);

        Task<TodoProductivityDTO> GetProgressForTodays(string UserId);
    }
}
