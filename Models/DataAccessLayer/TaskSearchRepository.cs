using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class TaskSearchRepository(ApplicationDbContext context) : ITaskSearchRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<TaskDTO>> SearchTasksExcludingRunningTodos(string userId, string query, Status status)
        {
            // Get today's date at UTC for a consistent time reference.
            var today = DateTime.UtcNow.Date;

            // Find all Task IDs that already have a Todo created for today.
            // We exclude these to prevent duplicate entries.
            var excludedTaskIds = await _context.Todo
                .Where(todo => todo.UserId == userId && todo.CreatedOn.Date == today)
                .Select(todo => todo.TaskId)
                .Distinct()
                .ToListAsync();

            // Start building the query for Tasks.
            var tasksQuery = _context.Tasks
                .Where(task => task.UserId == userId &&
                               !task.IsDeleted &&
                               !excludedTaskIds.Contains(task.Id) &&
                               (string.IsNullOrEmpty(query) || task.Name.Contains(query) || task.Id.ToString() == query));

            // Conditionally apply the status filter. If Status.All is passed, we skip this filter.
            if (status != Status.All)
            {
                tasksQuery = tasksQuery.Where(task => task.TaskStatus == status);
            }

            // Project the final result into the DTO.
            return await tasksQuery.Select(task => new TaskDTO
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Repeat = task.Repeat,
                RepeatWeekList = task.RepeatWeekList,
                Priority = task.Priority,
                EndDate = task.EndDate,
                StartDate = task.StartDate,
                IsScheduled = task.IsScheduled,
                StartOptionType = task.StartOptionType,
                IsStarted = task.IsStarted,
                UserId = task.UserId,
                TaskStatus = task.TaskStatus
            })
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskDTOWithGoalNameListDTO>> SearchTasksWithGoalsExcludingRunningTodosAsync(string userId, string query, Status status)
        {
            var today = DateTime.Now.Date;

            var excludedTaskIds = await _context.Todo
                .Where(todo => todo.UserId == userId && todo.CreatedOn.Date == today)
                .Select(todo => todo.TaskId)
                .Distinct()
                .ToListAsync();

            var tasksQuery = _context.Tasks
                .Where(task => task.UserId == userId &&
                               !task.IsDeleted &&
                               !excludedTaskIds.Contains(task.Id) &&
                               (string.IsNullOrEmpty(query) || task.Name.Contains(query) || task.Id.ToString() == query));

            if (status != Status.All)
            {
                tasksQuery = tasksQuery.Where(task => task.TaskStatus == status);
            }

            return await tasksQuery.Select(task => new TaskDTOWithGoalNameListDTO
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Repeat = task.Repeat,
                RepeatWeekList = task.RepeatWeekList,
                Priority = task.Priority,
                EndDate = task.EndDate,
                StartDate = task.StartDate,
                IsScheduled = task.IsScheduled,
                StartOptionType = task.StartOptionType,
                IsStarted = task.IsStarted,
                UserId = task.UserId,
                TaskStatus = task.TaskStatus,
                GoalLists = task.GoalTasks.Select(gt => new GoalNameDTO
                {
                    Id = gt.Goal.Id,
                    Name = gt.Goal.Name,
                    UserId = gt.Goal.UserId,
                    GoalStatus = gt.Goal.GoalStatus
                }).ToList()
            })
                .ToListAsync();
        }
    }
}