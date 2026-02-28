using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Exceptions;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Repositories;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class TaskRepository(ApplicationDbContext context) : ITaskRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task AddTasksAsync(string userId, TaskDTO tasks, string goalIds)
        {
            string repeatWeekListAsString = tasks.RepeatWeekList?
                .Select(w => ((int)w).ToString())
                .Aggregate("[", (current, next) => current + (current.Length > 1 ? "," : "") + next) + "]";


            var listOfParam = new List<SqlParameter>
            {
                new SqlParameter("@Name", tasks.Name),
                new SqlParameter("@StartDate", tasks.StartDate ?? (object)DBNull.Value),
                new SqlParameter("@StartOptionType", tasks.StartOptionType),
                new SqlParameter("@IsScheduled", tasks.IsScheduled),
                new SqlParameter("@EndDate", tasks.EndDate),
                new SqlParameter("@TaskStatus", tasks.TaskStatus),
                new SqlParameter("@Description", tasks.Description),
                new SqlParameter("@Priority", tasks.Priority),
                new SqlParameter("@Repeat", tasks.Repeat),
                new SqlParameter("@RepeatWeekList", repeatWeekListAsString ?? (object)DBNull.Value),
                new SqlParameter("@TaskId", -1),
                new SqlParameter("@GoalIds", goalIds ?? (object)DBNull.Value),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@Mode", 1) // create-operations
            };

            // Use the corrected parameter names without prefixes
            await _context.InsertUpdateSpWithIdDTO
                .FromSqlRaw(
                    "EXEC usp_AddUpdateTaskWithGoals @Name, @StartDate,@StartOptionType,@IsScheduled, @EndDate, @TaskStatus, @Description, @Priority, @Repeat, @RepeatWeekList, @TaskId, @GoalIds, @UserId, @Mode",
                    listOfParam.ToArray()
                )
                .ToListAsync();

        }

        public async Task DeleteTasksAsync(string UserId, int Id)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == Id && t.User.Id == UserId) ??
                throw new NotFoundException($"Task with ID {Id} not found.");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllTasksAsync<T>(string UserId, Status status, ResponseDataMode mode) where T : class
        {
            var baseQuery = _context.Tasks.Where(t => t.UserId == UserId && t.IsDeleted == false);

            if (status != Status.All) // Replaces conditional: (@Status IS NULL OR t.TaskStatus = @Status) assuming All = 4 acts as bypass.
            {
                baseQuery = baseQuery.Where(t => t.TaskStatus == status);
            }

            return mode switch
            {
                ResponseDataMode.Model => await baseQuery
                                            .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Tasks Data Not Found!"),

                ResponseDataMode.ModelDTO => await baseQuery
                                                .Select(t => new TaskDTO
                                                {
                                                    Id = t.Id,
                                                    Name = t.Name,
                                                    EndDate = t.EndDate,
                                                    Priority = t.Priority,
                                                    Repeat = t.Repeat,
                                                    RepeatWeekList = t.RepeatWeekList,
                                                    Description = t.Description,
                                                    TaskStatus = t.TaskStatus,
                                                    UserId = t.UserId,
                                                    IsScheduled = t.IsScheduled,
                                                    StartDate = t.StartDate,
                                                    IsStarted = t.IsStarted,
                                                    StartOptionType = t.StartOptionType,
                                                    CompletedOn = t.CompletedOn,
                                                    EndedOn = t.EndedOn
                                                })
                                                .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Tasks Data Not Found!"),

                ResponseDataMode.ModelNameDTO => await baseQuery
                                                    .Select(t => new TaskNameDTO
                                                    {
                                                        Id = t.Id,
                                                        Name = t.Name,
                                                        UserId = t.UserId
                                                    })
                                                    .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Tasks Data Not Found!"),

                _ => throw new InvalidOperationException("Invalid Operations While Fetching Tasks Data.")

            };
        }

        public async Task<T> GetTasksByIdAsync<T>(string userId, int id, ResponseDataMode mode) where T : class
        {
            var userIdParam = new SqlParameter("@UserId", userId);
            var taskIdParam = new SqlParameter("@TaskId", id);
            var modeParam = new SqlParameter("@Mode", mode);

            switch (mode)
            {
                case ResponseDataMode.Model:
                    var ModelData = await _context.Tasks
                        .FromSqlRaw("EXEC usp_GetTaskById @UserId, @TaskId, @Mode", userIdParam, taskIdParam, modeParam)
                        .ToListAsync() as IEnumerable<T>;

                    return ModelData?.FirstOrDefault()
                        ?? throw new NotFoundException($"Task with ID {id} not found or does not belong to user {userId}.");

                case ResponseDataMode.ModelDTO:
                    var ModelDTOData = await _context.TaskDTOs
                        .FromSqlRaw("EXEC usp_GetTaskById @UserId, @TaskId, @Mode", userIdParam, taskIdParam, modeParam)
                        .ToListAsync() as IEnumerable<T>;

                    return ModelDTOData?.FirstOrDefault()
                        ?? throw new NotFoundException($"Task with ID {id} not found or does not belong to user {userId}.");

                case ResponseDataMode.ModelNameDTO:
                    var ModelDTONameData = await _context.TaskNameDTOs
                        .FromSqlRaw("EXEC usp_GetTaskById @UserId, @TaskId, @Mode", userIdParam, taskIdParam, modeParam)
                        .ToListAsync() as IEnumerable<T>;

                    return ModelDTONameData?.FirstOrDefault()
                        ?? throw new NotFoundException($"Task with ID {id} not found or does not belong to user {userId}.");

                default:
                    throw new InvalidOperationException("Invalid Operations While Fetching Tasks Data.");
            }

        }

        public async Task<IEnumerable<T>> GetAllTasksWithStatusByGoalId<T>(string userId, int goalId, Status taskStatus, ResponseDataMode mode) where T : class
        {
            var baseQuery = _context.GoalTasks
                .Where(gt => gt.GoalId == goalId && gt.UserId == userId && gt.Task.UserId == userId && gt.Task.IsDeleted == false);

            if (taskStatus != Status.All) // Mapping 4 = All via enum bypass
            {
                baseQuery = baseQuery.Where(gt => gt.Task.TaskStatus == taskStatus);
            }

            return mode switch
            {
                ResponseDataMode.Model => await baseQuery
                                            .Select(gt => gt.Task)
                                            .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Tasks Data Not Found!"),

                ResponseDataMode.ModelDTO => await baseQuery
                                                .Select(gt => new TaskDTO
                                                {
                                                    Id = gt.Task.Id,
                                                    Name = gt.Task.Name,
                                                    EndDate = gt.Task.EndDate,
                                                    Priority = gt.Task.Priority,
                                                    Repeat = gt.Task.Repeat,
                                                    RepeatWeekList = gt.Task.RepeatWeekList,
                                                    Description = gt.Task.Description,
                                                    TaskStatus = gt.Task.TaskStatus,
                                                    UserId = gt.Task.UserId,
                                                    IsScheduled = gt.Task.IsScheduled,
                                                    StartDate = gt.Task.StartDate,
                                                    IsStarted = gt.Task.IsStarted,
                                                    StartOptionType = gt.Task.StartOptionType,
                                                    CompletedOn = gt.Task.CompletedOn,
                                                    EndedOn = gt.Task.EndedOn
                                                })
                                                .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Tasks Data Not Found!"),

                ResponseDataMode.ModelNameDTO => await baseQuery
                                                .Select(gt => new TaskNameDTO
                                                {
                                                    Id = gt.Task.Id,
                                                    Name = gt.Task.Name,
                                                    UserId = gt.Task.UserId
                                                })
                                                .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Tasks Data Not Found!"),

                _ => throw new InvalidOperationException("Invalid Operations While Fetching Tasks Data.")
            };
        }

        public async Task UpdateTasksAsync(string userId, Tasks tasks, string goalIds)
        {
            string repeatWeekListAsString = tasks.RepeatWeekList?
                .Select(w => ((int)w).ToString())
                .Aggregate("[", (current, next) => current + (current.Length > 1 ? "," : "") + next) + "]";


            var listOfParam = new List<SqlParameter>
            {
                new SqlParameter("@Name", tasks.Name),
                new SqlParameter("@StartDate", tasks.StartDate ?? (object)DBNull.Value),
                new SqlParameter("@StartOptionType", tasks.StartOptionType),
                new SqlParameter("@IsScheduled", tasks.IsScheduled),
                new SqlParameter("@EndDate", tasks.EndDate),
                new SqlParameter("@TaskStatus", tasks.TaskStatus),
                new SqlParameter("@Description", tasks.Description),
                new SqlParameter("@Priority", tasks.Priority),
                new SqlParameter("@Repeat", tasks.Repeat),
                new SqlParameter("@RepeatWeekList", repeatWeekListAsString ?? (object)DBNull.Value),
                new SqlParameter("@TaskId", tasks.Id),
                new SqlParameter("@GoalIds", goalIds ?? (object)DBNull.Value),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@Mode", 2) // update-operations
            };

            // Use the corrected parameter names without prefixes
            await _context.InsertUpdateSpWithIdDTO
                .FromSqlRaw(
                    "EXEC usp_AddUpdateTaskWithGoals @Name, @StartDate,@StartOptionType,@IsScheduled, @EndDate, @TaskStatus, @Description, @Priority, @Repeat, @RepeatWeekList, @TaskId, @GoalIds, @UserId, @Mode",
                    listOfParam.ToArray()
                )
                .ToListAsync();
        }

        public async Task UpdateTasksAsync(string UserId, Tasks tasks)
        {
            var user = tasks.UserId ?? throw new ArgumentNullException("UserId Is Required!.");

            if (user != UserId)
            {
                throw new ArgumentException("UserId and Task.User.Id must be same!.");
            }
            _context.Tasks.Update(tasks);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskStatusAsync(string userId, int taskId, Status statusToChange)
        {
            var currentDateTime = DateTime.Now;

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId && t.IsDeleted == false && t.EndDate > currentDateTime);

            if (task == null)
            {
                throw new NotFoundException("Task Not Found! Task Must be in Active State");
            }

            var currentTaskStatus = task.TaskStatus;

            if (statusToChange == Status.NotStarted)
            {
                if (currentTaskStatus == Status.Running)
                {
                    task.TaskStatus = Status.NotStarted;
                    task.UpdatedOn = currentDateTime;
                }
                else
                {
                    throw new InvalidOperationException("Task must be in running state!");
                }
            }
            else if (statusToChange == Status.Running)
            {
                if (currentTaskStatus == Status.NotStarted || currentTaskStatus == Status.Completed || currentTaskStatus == Status.Ended)
                {
                    task.TaskStatus = Status.Running;
                    task.UpdatedOn = currentDateTime;

                    if (currentTaskStatus == Status.NotStarted && !task.IsStarted)
                    {
                        task.IsStarted = true;
                        task.StartedOn = currentDateTime;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Task must be in Active State!");
                }
            }
            else if (statusToChange == Status.Completed)
            {
                if (currentTaskStatus == Status.Running)
                {
                    task.TaskStatus = Status.Completed;
                    task.UpdatedOn = currentDateTime;
                    task.CompletedOn = currentDateTime;
                }
                else
                {
                    throw new InvalidOperationException("Task must be in running state!");
                }
            }
            else if (statusToChange == Status.Ended)
            {
                if (currentTaskStatus == Status.Running)
                {
                    task.TaskStatus = Status.Ended;
                    task.UpdatedOn = currentDateTime;
                    task.EndedOn = currentDateTime;
                }
                else
                {
                    throw new InvalidOperationException("Task must be in running state!");
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid Task Status!");
            }

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTaskCountByTaskStatus(string UserId, Status status)
        {
            return await _context.Tasks
                .CountAsync(t => t.UserId == UserId
                && t.TaskStatus == status
                && t.IsDeleted == false);
        }

        public async Task<int> GetTaskCountByTaskStatusAndDateTimeRange(string UserId, Status status, DateTime from, DateTime end)
        {
            if (from > end)
            {
                throw new InvalidOperationException("Invalid DateTime Range!");
            }

            // we are just checking the last update... this logic is not 100% correct so change according..
            return await _context.Tasks.CountAsync(g => g.User.Id == UserId && (g.TaskStatus == status && g.UpdatedOn >= from && g.UpdatedOn <= end));
        }

        public async Task<int> GetTaskCountByTaskStatus(string UserId, int goalId, Status status)
        {
            return await _context.Tasks
                .Include(t => t.GoalTasks)
                .CountAsync(t => t.UserId == UserId
                    && t.TaskStatus == status
                    && t.GoalTasks.Any(gt => gt.GoalId == goalId)
                    && t.IsDeleted == false);

        }

        public async Task<int> GetTaskCountByTaskStatusAndDateTimeRange(string UserId, int goalId, Status status, DateTime from, DateTime end)
        {
            if (from > end)
            {
                throw new InvalidOperationException("Invalid DateTime Range!");
            }

            // we are just checking the last update... this logic is not 100% correct so change according..
            return await _context.Tasks
                .CountAsync(g =>
                    g.User.Id == UserId
                    && g.TaskStatus == status
                    && g.UpdatedOn >= from
                    && g.UpdatedOn <= end
                    && g.GoalTasks.Any(gt => gt.GoalId == goalId)
                    && g.IsDeleted == false);
        }

        public async Task<IEnumerable<TaskNameDTO>> SearchTasks(string userId, string query)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId && 
                            (t.Name.Contains(query) || t.Id.ToString() == query) && 
                            !t.IsDeleted)
                .Select(t => new TaskNameDTO
                {
                    Id = t.Id,
                    Name = t.Name,
                    UserId = t.UserId
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskDTOWithGoalNameListDTO>> SearchTasksWithGoals(string userId, string query, Status status)
        {
            var tasksQuery = _context.Tasks
                .Include(t => t.GoalTasks)
                 .ThenInclude(gt => gt.Goal)
                .Where(t => t.UserId == userId && !t.IsDeleted);

            if (!string.IsNullOrWhiteSpace(query))
            {
                tasksQuery = tasksQuery.Where(t => t.Name.Contains(query) || t.Id.ToString() == query);
            }

            if (status != Status.All)
            {
                tasksQuery = tasksQuery.Where(t => t.TaskStatus == status);
            }

            var tasks = await tasksQuery.ToListAsync();

            var result = tasks.Select(t => new TaskDTOWithGoalNameListDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Repeat = t.Repeat,
                RepeatWeekList = t.RepeatWeekList,
                Priority = t.Priority,
                EndDate = t.EndDate,
                StartDate = t.StartDate,
                IsScheduled = t.IsScheduled,
                StartOptionType = t.StartOptionType,
                IsStarted = t.IsStarted,
                UserId = t.UserId,
                TaskStatus = t.TaskStatus,
                GoalLists = t.GoalTasks?.Select(gt => new GoalNameDTO
                {
                    Id = gt.GoalId,
                    Name = gt.Goal?.Name,
                    UserId = gt.Goal?.UserId,
                    GoalStatus = gt.Goal?.GoalStatus
                }).ToList()
            });

            return result;
        }

        public async Task<int> AddUpdateTaskWithGoalsAsync(string userId, TaskDTO taskDto, IEnumerable<int> goalIds, int mode)
        {
            var currentDateTime = DateTime.Now;
            int returnedTaskId = taskDto.Id;

            // Mode 1: Add
            if (mode == 1)
            {
                var isStartedCalculated = taskDto.IsScheduled && taskDto.StartDate <= currentDateTime;
                var newTask = new Tasks
                {
                    Name = taskDto.Name,
                    StartDate = taskDto.StartDate,
                    IsScheduled = taskDto.IsScheduled,
                    StartOptionType = taskDto.StartOptionType,
                    IsStarted = isStartedCalculated,
                    StartedOn = isStartedCalculated ? currentDateTime : null,
                    EndDate = taskDto.EndDate.Value,
                    CreatedOn = currentDateTime,
                    UpdatedOn = currentDateTime,
                    TaskStatus = isStartedCalculated ? Status.Running : taskDto.TaskStatus,
                    Description = taskDto.Description,
                    Priority = taskDto.Priority,
                    Repeat = taskDto.Repeat,
                    RepeatWeekList = taskDto.RepeatWeekList,
                    UserId = userId
                };

                await _context.Tasks.AddAsync(newTask);
                await _context.SaveChangesAsync();

                returnedTaskId = newTask.Id;
            }
            // Mode 2: Update
            else if (mode == 2)
            {
                var existingTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskDto.Id && t.UserId == userId && !t.IsDeleted);
                if (existingTask == null)
                {
                    throw new NotFoundException("Task Data Not Found!");
                }

                existingTask.Name = taskDto.Name;
                existingTask.EndDate = taskDto.EndDate.Value;
                existingTask.UpdatedOn = currentDateTime;
                existingTask.TaskStatus = taskDto.TaskStatus;
                existingTask.Description = taskDto.Description;
                existingTask.Priority = taskDto.Priority;
                existingTask.Repeat = taskDto.Repeat;
                existingTask.RepeatWeekList = taskDto.RepeatWeekList;

                _context.Tasks.Update(existingTask);
                await _context.SaveChangesAsync();
            }

            // Relationship updates
            var incomingGoalIds = goalIds?.ToList() ?? new List<int>();

            // Delete relations not in incoming list
            var relationsToRemove = await _context.GoalTasks
                .Where(gt => gt.TaskId == returnedTaskId && gt.UserId == userId && !incomingGoalIds.Contains(gt.GoalId))
                .ToListAsync();

            if (relationsToRemove.Any())
            {
                _context.GoalTasks.RemoveRange(relationsToRemove);
            }

            // Add new relations
            if (incomingGoalIds.Any())
            {
                var existingRelationGoalIds = await _context.GoalTasks
                    .Where(gt => gt.TaskId == returnedTaskId && gt.UserId == userId)
                    .Select(gt => gt.GoalId)
                    .ToListAsync();

                var newGoalIds = incomingGoalIds.Except(existingRelationGoalIds).ToList();

                if (newGoalIds.Any())
                {
                    var validGoals = await _context.Goals
                        .Where(g => newGoalIds.Contains(g.Id) && 
                                    (g.GoalStatus == Status.Running || g.GoalStatus == Status.NotStarted) && 
                                    !g.IsDeleted && 
                                    g.EndDate > currentDateTime)
                        .Select(g => g.Id)
                        .ToListAsync();

                    var newRelations = validGoals.Select(goalId => new GoalTask
                    {
                        GoalId = goalId,
                        TaskId = returnedTaskId,
                        UserId = userId
                    });

                    await _context.GoalTasks.AddRangeAsync(newRelations);
                }
            }

            await _context.SaveChangesAsync();
            return returnedTaskId;
        }
    }
}
