using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System;
using System.Threading.Tasks;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Utility;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.Data.SqlClient;
using TaskMonitoringApp.Models.DTOs;
using AutoMapper;
using TaskMonitoringApp.Exceptions;
using System.Collections;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class TodoRepository(ApplicationDbContext context) : ITodoRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task AddTodoAsync(string UserId, Todo todo)
        {
            var user = todo.User ?? throw new ArgumentNullException(nameof(todo.User), "UserId Is Required!.");

            if (user.Id != UserId)
            {
                throw new ArgumentException("UserId and Todo.User.Id must be same!.");
            }

            await _context.Todo.AddAsync(todo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTodoAsync(string UserId, int Id)
        {
            var todo = await _context.Todo.FirstOrDefaultAsync(T => T.User.Id == UserId && T.Id == Id) ??
                throw new InvalidOperationException("Error: attempts to delete todo that does not exits."); ;

            _context.Todo.Remove(todo);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllTodoAsync<T>(string userId, Status status, ResponseDataMode mode) where T : class
        {
            var userIdParam = new SqlParameter("@UserId", userId);
            var statusParam = new SqlParameter("@Status", status);
            var modeParam = new SqlParameter("@Mode", mode);

            return mode switch
            {
                ResponseDataMode.Model => await _context.TodoWithTask
                        .FromSqlRaw("EXEC usp_AddAndGetTodoFromTask @UserId, @Status, @Mode", userIdParam, statusParam, modeParam)
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Todo Data Not Found!"),
                ResponseDataMode.ModelDTO => await _context.TodoWithTaskDTO
                        .FromSqlRaw("EXEC usp_AddAndGetTodoFromTask @UserId, @Status, @Mode", userIdParam, statusParam, modeParam)
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Todo Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Todo Data.")
            };
        }

        public async Task<IEnumerable<T>> GetAllTodoAsync<T>(string userId, Status status, DateTime selectDate, ResponseDataMode mode) where T : class
        {

            return mode switch
            {
                ResponseDataMode.Model => await _context.Todo
                    .Where(todo => todo.UserId == userId && todo.CreatedOn >= selectDate.Date && todo.CreatedOn < selectDate.AddDays(1).Date && todo.Status == status && !todo.IsDeleted)
                    .ToListAsync() as IEnumerable<T>
                    ?? throw new NotFoundException("Todo Data Not Found!"),
                ResponseDataMode.ModelDTO => await _context.Todo
                        .Where(todo => todo.UserId == userId && todo.CreatedOn >= selectDate.Date && todo.CreatedOn < selectDate.AddDays(1).Date && todo.Status == status && !todo.IsDeleted)
                        .Include(todo => todo.Task)
                        .Select(todo => new TodoDTOWithTaskDTO()
                        {
                            Id = todo.Id,
                            EndDate = todo.EndDate,
                            Status = todo.Status,
                            TaskId = todo.TaskId,
                            UserId = todo.UserId,
                            TaskName = todo.Task.Name,
                            Notes = todo.Notes,
                            TaskDescription = todo.Task.Description,
                            TaskRepeat = todo.Task.Repeat,
                            TaskRepeatWeekList = todo.Task.RepeatWeekList,
                            TaskPriority = todo.Task.Priority,
                            TaskEndDate = todo.Task.EndDate,
                            TaskStatus = todo.Task.TaskStatus,
                            IsManualAdded = todo.IsManualAdded,
                            TaskCompletedOn = todo.Task.CompletedOn,
                            TaskEndedOn = todo.Task.EndedOn
                            
                        }).ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Todo Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Todo Data.")
            };
        }

        public async Task<IEnumerable<T>> GetAllTodoAsync<T>(string userId, int taskId, Status status, ResponseDataMode mode) where T : class
        {
            var selectDate = DateTime.Now;

            return mode switch
            {
                ResponseDataMode.Model => await _context.Todo
                    .Where(todo => todo.UserId == userId && todo.TaskId == taskId && todo.CreatedOn >= selectDate.Date && todo.CreatedOn < selectDate.AddDays(1).Date && todo.Status == status && !todo.IsDeleted)
                    .ToListAsync() as IEnumerable<T>
                    ?? throw new NotFoundException("Todo Data Not Found!"),
                ResponseDataMode.ModelDTO => await _context.Todo
                        .Where(todo => todo.UserId == userId && todo.TaskId == taskId && todo.CreatedOn >= selectDate.Date && todo.CreatedOn < selectDate.AddDays(1).Date && todo.Status == status && !todo.IsDeleted)
                        .Include(todo => todo.Task)
                        .Select(todo => new TodoDTOWithTaskDTO()
                        {
                            Id = todo.Id,
                            EndDate = todo.EndDate,
                            Status = todo.Status,
                            TaskId = todo.TaskId,
                            UserId = todo.UserId,
                            Notes = todo.Notes,
                            TaskName = todo.Task.Name,
                            TaskDescription = todo.Task.Description,
                            TaskRepeat = todo.Task.Repeat,
                            TaskRepeatWeekList = todo.Task.RepeatWeekList,
                            TaskPriority = todo.Task.Priority,
                            TaskEndDate = todo.Task.EndDate,
                            TaskStatus = todo.Task.TaskStatus,
                            IsManualAdded = todo.IsManualAdded,
                            TaskCompletedOn = todo.Task.CompletedOn,
                            TaskEndedOn = todo.Task.EndedOn

                        }).ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Todo Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Todo Data.")
            };
        }

        public async Task<IEnumerable<T>> GetAllTodoAsync<T>(string userId, int taskId, Status status, DateTime selectDate, ResponseDataMode mode) where T : class
        {

            return mode switch
            {
                ResponseDataMode.Model => await _context.Todo
                    .Where(todo => todo.UserId == userId && todo.TaskId == taskId && todo.CreatedOn >= selectDate.Date && todo.CreatedOn < selectDate.AddDays(1).Date && todo.Status == status && !todo.IsDeleted)
                    .ToListAsync() as IEnumerable<T>
                    ?? throw new NotFoundException("Todo Data Not Found!"),
                ResponseDataMode.ModelDTO => await _context.Todo
                        .Where(todo => todo.UserId == userId && todo.TaskId == taskId && todo.CreatedOn >= selectDate.Date && todo.CreatedOn < selectDate.AddDays(1).Date && todo.Status == status && !todo.IsDeleted)
                        .Include(todo => todo.Task)
                        .Select(todo => new TodoDTOWithTaskDTO()
                        {
                            Id = todo.Id,
                            EndDate = todo.EndDate,
                            Status = todo.Status,
                            TaskId = todo.TaskId,
                            UserId = todo.UserId,
                            TaskName = todo.Task.Name,
                            Notes = todo.Notes,
                            TaskDescription = todo.Task.Description,
                            TaskRepeat = todo.Task.Repeat,
                            TaskRepeatWeekList = todo.Task.RepeatWeekList,
                            TaskPriority = todo.Task.Priority,
                            TaskEndDate = todo.Task.EndDate,
                            TaskStatus = todo.Task.TaskStatus,
                            IsManualAdded = todo.Task.IsScheduled,
                            TaskEndedOn = todo.Task.EndedOn,
                            TaskCompletedOn = todo.Task.CompletedOn

                        }).ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Todo Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Todo Data.")
            };
        }

        // Todo: Need to test this methods....
        public async Task<IEnumerable<T>> GetAllTodoWithStatusByGoalId<T>(string userId, int goalId, Status todoStatus, ResponseDataMode mode) where T : class
        {
            return mode switch
            {
                ResponseDataMode.Model => await _context.Todo
                        .Include(td => td.Task)
                        .ThenInclude(t => t.GoalTasks)
                        .Where(td => td.UserId == userId
                            && td.Status == todoStatus
                            && td.IsDeleted == false
                            && td.Task.GoalTasks.Any(gt => gt.GoalId == goalId))
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Todo Data Not Found!"),
                ResponseDataMode.ModelDTO => await _context.Todo
                    .Include(td => td.Task)
                        .ThenInclude(t => t.GoalTasks)
                        .Where(td => td.UserId == userId
                            && td.Status == todoStatus
                            && td.IsDeleted == false
                            && td.Task.GoalTasks.Any(gt => gt.GoalId == goalId))
                        .Select(td => new TodoDTO()
                        {
                            Id = td.Id,
                            EndDate = td.EndDate,
                            Status = td.Status,
                            UserId = td.UserId,
                            TaskId = td.Task.Id,
                            IsManualAdded = td.IsManualAdded,

                            Task = new TaskDTO()
                            {
                                Id = td.Task.Id,
                                Name = td.Task.Name,
                                Description = td.Task.Description,
                                EndDate = td.Task.EndDate,
                                Priority = td.Task.Priority,
                                Repeat = td.Task.Repeat,
                                RepeatWeekList = td.Task.RepeatWeekList,
                                TaskStatus = td.Task.TaskStatus,
                                UserId = td.Task.UserId,
                                CompletedOn = td.Task.CompletedOn,
                                EndedOn = td.Task.EndedOn
                            }
                        })
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Todo Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Todo Data.")

            };
        }

        // Todo: Need to test this methods.....
        public async Task<IEnumerable<T>> GetAllTodoWithStatusByTaskId<T>(string userId, int taskId, Status todoStatus, ResponseDataMode mode) where T : class
        {
            return mode switch
            {
                ResponseDataMode.Model => await _context.Todo
                    .Include(td => td.Task)
                    .Where(td => td.UserId == userId && td.IsDeleted == false && td.Status == todoStatus && td.Task.Id == taskId)
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Todo Data Not Found!"),
                ResponseDataMode.ModelDTO => await _context.Todo
                    .Include(td => td.Task)
                    .Where(td => td.UserId == userId && td.IsDeleted == false && td.Status == todoStatus && td.Task.Id == taskId)
                    .Select(td => new TodoDTO()
                    {
                        Id = td.Id,
                        TaskId = td.Task.Id,
                        UserId = td.UserId,
                        EndDate = td.EndDate,
                        Status = td.Status,
                        IsManualAdded = td.IsManualAdded,
                        Task = new TaskDTO()
                        {
                            Id = td.Task.Id,
                            Name = td.Task.Name,
                            Description = td.Task.Description,
                            TaskStatus = td.Task.TaskStatus,
                            EndDate = td.Task.EndDate,
                            Priority = td.Task.Priority,
                            Repeat = td.Task.Repeat,
                            RepeatWeekList = td.Task.RepeatWeekList,
                            UserId = td.Task.UserId,
                            CompletedOn = td.CompletedOn,
                            EndedOn = td.EndedOn
                        }
                    })
                    .AsSingleQuery()
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Todo Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Todo Data.")
            };
        }

        public async Task<T> GetTodoByIdAsync<T>(string UserId, int Id, ResponseDataMode mode) where T : class
        {
            switch (mode)
            {
                case ResponseDataMode.Model:
                    var modelData = await _context.Todo
                            .Where(td => td.UserId == UserId && td.Id == Id && td.IsDeleted == false)
                            .FirstOrDefaultAsync() as T;

                    if (modelData == null)
                    {
                        throw new NotFoundException("Todo Data Not Found!");
                    }

                    return modelData;

                case ResponseDataMode.ModelDTO:
                    var modelDTOData = await _context.Todo
                            .Where(td => td.UserId == UserId && td.Id == Id && td.IsDeleted == false)
                            .Select(td => new TodoDTO()
                            {
                                Id = td.Id,
                                EndDate = td.EndDate,
                                Status = td.Status,
                                UserId = td.UserId,
                                Notes = td.Notes,
                                TaskId = td.Task.Id,
                                IsManualAdded = td.IsManualAdded,
                                Task = new TaskDTO()
                                {
                                    Id = td.Task.Id,
                                    Name = td.Task.Name,
                                    Description = td.Task.Description,
                                    EndDate = td.Task.EndDate,
                                    Priority = td.Task.Priority,
                                    Repeat = td.Task.Repeat,
                                    RepeatWeekList = td.Task.RepeatWeekList,
                                    TaskStatus = td.Task.TaskStatus,
                                    UserId = td.Task.UserId,
                                    CompletedOn = td.Task.CompletedOn,
                                    EndedOn = td.Task.EndedOn
                                }
                            })
                            .FirstOrDefaultAsync() as T; 

                    if (modelDTOData == null)
                    {
                        throw new NotFoundException("Todo Data Not Found!");
                    }

                    return modelDTOData; 

                default:
                    throw new InvalidOperationException("Invalid Operations While Fetching Todo Data.");

            }
        }

        public async Task UpdateTodoAsync(string UserId, Todo todo)
        {
            var user = todo.User ?? throw new ArgumentNullException(nameof(todo.User), "UserId Is Required!.");

            if (user.Id != UserId)
            {
                throw new ArgumentException("UserId and Todo.User.Id must be same!.");
            }

            _context.Todo.Update(todo);
            await _context.SaveChangesAsync();
        }

        public async Task<T> GetTodoProgressAnalysesAsync<T>(string userId, ResponseDataMode mode) where T : class
        {
            var userIdParam = new SqlParameter("@UserId", userId);
            var modeParam = new SqlParameter("@Mode", mode);

            switch (mode)
            {
                case ResponseDataMode.Model:
                    var todayAnalysisModel = await _context.TodoProgressAnalyses
                        .FromSqlRaw("EXEC usp_TodoProgressAnalyses @UserId, @Mode", userIdParam, modeParam)
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Todo Not Found!");

                    return todayAnalysisModel.FirstOrDefault()
                            ?? throw new NotFoundException("Todo Data Not Found!"); ;

                case ResponseDataMode.ModelDTO:
                    var todayAnalysisDTO = await _context.TodoProgressAnalysesDTO
                    .FromSqlRaw("EXEC usp_TodoProgressAnalyses @UserId, @Mode", userIdParam, modeParam)
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Todo Not Found!");

                    return todayAnalysisDTO.FirstOrDefault()
                            ?? throw new NotFoundException("Todo Data Not Found!"); ;

                default: throw new InvalidOperationException("Invalid Operations While Fetching Todo Data.");
            }
            ;
        }

        public async Task<T> GetTodoProgressAnalysesAsync<T>(string userId, DateTime forDate, ResponseDataMode mode) where T : class
        {
            switch (mode)
            {
                case ResponseDataMode.Model:
                    var todayAnalysisModel = await _context.TodoProgressAnalyses
                        .Where(tpa => tpa.UserId == userId && !tpa.IsDeleted && tpa.CalculateDateFor >= forDate.Date && tpa.CalculateDateFor < forDate.AddDays(1).Date)
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Todo Analyses Not Found!");

                    return todayAnalysisModel.FirstOrDefault()
                            ?? throw new NotFoundException("Todo Analyses Data Not Found!"); ;

                case ResponseDataMode.ModelDTO:
                    var todayAnalysisDTO = await _context.TodoProgressAnalyses
                    .Where(tpa => tpa.UserId == userId && !tpa.IsDeleted && tpa.CalculateDateFor >= forDate.Date && tpa.CalculateDateFor < forDate.AddDays(1).Date)
                    .Select(tpa => new TodoProgressAnalysisDTO()
                    {
                        Id = tpa.Id,
                        CalculateDateFor = tpa.CalculateDateFor,
                        TotalTodo = tpa.TotalTodo,
                        TotalCompletedTodo = tpa.TotalCompletedTodo,
                        TotalMissedTodo = tpa.TotalMissedTodo,
                        ProductivityForDay = tpa.ProductivityForDay,
                        UserId = tpa.UserId
                    })
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Todo Analyses Not Found!");

                    return todayAnalysisDTO.FirstOrDefault()
                            ?? throw new NotFoundException("Todo Analyses Data Not Found!"); ;

                default: throw new InvalidOperationException("Invalid Operations While Fetching Todo Analyses Data.");
            }
            ;
        }

        public async Task<TodoProgressAnalysisDTO> GetTodoProgressAnalysesAsync(string userId, int taskId)
        {
            var totalTodos = await _context.Todo.CountAsync(t => t.TaskId == taskId && t.UserId == userId);
            var completedTodos = await _context.Todo.CountAsync(t => t.TaskId == taskId && t.UserId == userId && t.Status == Status.Completed);

            double productivity = 0d;

            if (totalTodos > 0)
            {
                productivity = Math.Round((double)completedTodos / totalTodos * 100, 2);
            }

            var data = new TodoProgressAnalysisDTO
            {
                TotalTodo = totalTodos,
                TotalCompletedTodo = completedTodos,
                TotalMissedTodo = totalTodos - completedTodos,
                ProductivityForDay = productivity,
                CalculateDateFor = DateTime.Today,
                UserId = userId
            };



            return data;
        }


        // todo need to implement this metods....
        public async Task UpdateTodoStatusAsync(string userId, int todoId, Status statusToChange)
        {
            var userIdParam = new SqlParameter("@UserId", userId);
            var todoIdParam = new SqlParameter("@TodoId", todoId);
            var statusToChangeParam = new SqlParameter("@StatusToUpdate", statusToChange);

            await _context.InsertUpdateSpWithIdDTO.FromSqlRaw("EXEC usp_ChangeTodoStatus @UserId, @TodoId, @StatusToUpdate", userIdParam, todoIdParam, statusToChangeParam)
                .ToListAsync();
        }

        public async Task UpdateTodoNotesAsync(string userId, int todoId, string notes)
        {
            var todo = await _context.Todo.FirstOrDefaultAsync(t => t.UserId == userId && t.Id == todoId && !t.IsDeleted)
                ?? throw new NotFoundException("Todo not found!");

            todo.Notes = notes;
            todo.UpdatedOn = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task AddBulkTodosAsync(BulkTodoCreateDTO bulkDto)
        {
            if (bulkDto.TaskIds == null || !bulkDto.TaskIds.Any())
                throw new ArgumentException("No task IDs provided for bulk todo creation.");

            var todos = bulkDto.TaskIds.Select(taskId => new Todo
            {
                TaskId = taskId,
                UserId = bulkDto.UserId,
                EndDate = DateTime.Now.AddDays(1),
                Status = bulkDto.Status,
                IsManualAdded = true,
                Notes = string.Empty,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now,
                IsDeleted = false
            }).ToList();

            await _context.Todo.AddRangeAsync(todos);
            await _context.SaveChangesAsync();
        }
    }
}
