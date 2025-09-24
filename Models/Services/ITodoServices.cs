using System.Threading.Tasks;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TodoMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Services
{
    public interface ITodoServices
    {
        Task AddNewTodo (string UserId, TodoDTO todo);

        Task<TodoDTOWithTaskDTO> GetTodoById(string UserId, int id);

        Task<IEnumerable<TodoDTOWithTaskDTO>> GetAllTodo(string UserId, Status status);

        Task<IEnumerable<TodoDTOWithTaskDTO>> GetAllTodo(string UserId, Status status, DateTime selectDate);

        Task<IEnumerable<TodoDTOWithTaskDTO>> GetAllTodo(string UserId);

        Task<IEnumerable<TodoDTOWithTaskDTO>> GetAllTodo(string UserId, int taskId, Status status);

        Task<IEnumerable<TodoDTOWithTaskDTO>> GetAllTodo(string UserId, int taskId, Status status, DateTime selectDate);

        Task<IEnumerable<TodoDTOWithTaskDTO>> GetAllTodo(string UserId, int taskId);

        Task DeleteTodoById(string UserId, int id);

        Task UpdateTodo(string UserId, TodoDTO todo);

        Task UpdateTodoStatus(string userId, int todoId, Status statusToChange);

        Task<TodoProgressAnalysisDTO> GetTodoProgressAnalyses(string userId, DateTime forDate);
        Task<TodoProgressAnalysisDTO> GetTodoProgressAnalyses(string userId, int taskId);
    }
}
