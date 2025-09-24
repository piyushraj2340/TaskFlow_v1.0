using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Exceptions;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
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
            var userIdParam = new SqlParameter("@UserId", UserId);
            var statusParam = new SqlParameter("@Status", status);
            var modeParam = new SqlParameter("@Mode", mode);

            return mode switch
            {
                ResponseDataMode.Model => await _context.Tasks
                                            .FromSqlRaw("EXEC usp_GetAllTasksWithStatus @UserId, @Status, @Mode", userIdParam, statusParam, modeParam)
                                            .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Tasks Data Not Found!"),

                ResponseDataMode.ModelDTO => await _context.TaskDTOs
                                                .FromSqlRaw("EXEC usp_GetAllTasksWithStatus @UserId, @Status, @Mode", userIdParam, statusParam, modeParam)
                                                .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Tasks Data Not Found!"),

                ResponseDataMode.ModelNameDTO => await _context.TaskNameDTOs
                                                    .FromSqlRaw("EXEC usp_GetAllTasksWithStatus @UserId, @Status, @Mode", userIdParam, statusParam, modeParam)
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
            var userIdParam = new SqlParameter("@UserId", userId);
            var taskStatusParam = new SqlParameter("@Status", taskStatus);
            var goalIdParam = new SqlParameter("@GoalId", goalId);
            var modeParam = new SqlParameter("@Mode", mode);

            return mode switch
            {
                ResponseDataMode.Model => await _context.Tasks
                                            .FromSqlRaw("EXEC usp_GetAllTasksWithStatusByGoalId @UserId, @Status, @GoalId, @Mode", userIdParam, taskStatusParam, goalIdParam, modeParam)
                                            .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Tasks Data Not Found!"),

                ResponseDataMode.ModelDTO => await _context.TaskDTOs
                                                .FromSqlRaw("EXEC usp_GetAllTasksWithStatusByGoalId @UserId, @Status, @GoalId, @Mode", userIdParam, taskStatusParam, goalIdParam, modeParam)
                                                .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Tasks Data Not Found!"),

                ResponseDataMode.ModelNameDTO => await _context.TaskNameDTOs
                                                .FromSqlRaw("EXEC usp_GetAllTasksWithStatusByGoalId @UserId, @Status, @GoalId, @Mode", userIdParam, taskStatusParam, goalIdParam, modeParam)
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
            var userIdParam = new SqlParameter("@UserId", userId);
            var taskIdParam = new SqlParameter("@TaskId", taskId);
            var statusToChangeParam = new SqlParameter("@StatusToUpdate", statusToChange);

            await _context.InsertUpdateSpWithIdDTO.FromSqlRaw("EXEC usp_ChangeTaskStatus @UserId, @TaskId, @StatusToUpdate", userIdParam, taskIdParam, statusToChangeParam)
                .ToListAsync();
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
    }
}
