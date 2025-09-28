using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Models.Business
{
    public class TaskSearchService(ITaskSearchRepository repository) : ITaskSearchService
    {
        private readonly ITaskSearchRepository _repository = repository;

        public async Task<IEnumerable<TaskDTO>> SearchTasksExcludingRunningTodos(string userId, string query, Status status)
        {
            return await _repository.SearchTasksExcludingRunningTodos(userId, query, status);
        }

        public async Task<IEnumerable<TaskDTOWithGoalNameListDTO>> SearchTasksWithGoalsExcludingRunningTodosAsync(string userId, string query, Status status)
        {
            return await _repository.SearchTasksWithGoalsExcludingRunningTodosAsync(userId, query, status);
        }
    }
}