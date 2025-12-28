using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Data.Common;
using TaskMonitoringApp.Exceptions;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class GoalRepository(ApplicationDbContext context) : IGoalRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<T>> GetAllGoalsAsync<T>(string UserId, Status status, ResponseDataMode mode) where T : class
        {
             var userIdParam = new SqlParameter("@UserId", UserId);
            var statusParam = new SqlParameter("@Status", status);
            var modeParam = new SqlParameter("@Mode", mode);

            return mode switch
            {
                ResponseDataMode.Model => await _context.Goals
                                            .FromSqlRaw("EXEC usp_GetAllGoalsWithStatus @UserId, @Status, @Mode", userIdParam, statusParam, modeParam)
                                            .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                ResponseDataMode.ModelDTO => await _context.GoalDTOs
                                                .FromSqlRaw("EXEC usp_GetAllGoalsWithStatus @UserId, @Status, @Mode", userIdParam, statusParam, modeParam)
                                                .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                ResponseDataMode.ModelNameDTO => await _context.GoalNameDTOs
                                                    .FromSqlRaw("EXEC usp_GetAllGoalsWithStatus @UserId, @Status, @Mode", userIdParam, statusParam, modeParam)
                                                    .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                _ => throw new InvalidOperationException("Invalid Operations While Fetching Goals Data.")
            };
        }

        public async Task<T> GetGoalByIdAsync<T>(string userId, int id, ResponseDataMode mode) where T : class
        {
             var userIdParam = new SqlParameter("@UserId", userId);
            var goalIdParam = new SqlParameter("@GoalId", id);
            var modeParam = new SqlParameter("@Mode", mode);


            switch (mode)
            {
                case ResponseDataMode.Model:

                    var dataGoalModel = await _context.Goals.FromSqlRaw("EXEC usp_GetGoalById @UserId, @GoalId, @Mode", userIdParam, goalIdParam, modeParam)
                            .ToListAsync() as IEnumerable<T>;

                    return dataGoalModel?.FirstOrDefault()
                            ?? throw new NotFoundException($"Goal with ID {id} not found or does not belong to user {userId}.");

                case ResponseDataMode.ModelDTO:

                    var dataGoalDTO = await _context.GoalDTOs.FromSqlRaw("EXEC usp_GetGoalById @UserId, @GoalId, @Mode", userIdParam, goalIdParam, modeParam)
                                .ToListAsync() as IEnumerable<T>;

                    return dataGoalDTO?.FirstOrDefault()
                            ?? throw new NotFoundException($"Goal with ID {id} not found or does not belong to user {userId}.");

                case ResponseDataMode.ModelNameDTO:

                    var dataGoalNameDTO = await _context.GoalNameDTOs.FromSqlRaw("EXEC usp_GetGoalById @UserId, @GoalId, @Mode", userIdParam, goalIdParam, modeParam)
                                .ToListAsync() as IEnumerable<T>;

                    return dataGoalNameDTO?.FirstOrDefault()
                            ?? throw new NotFoundException($"Goal with ID {id} not found or does not belong to user {userId}.");

                default: throw new InvalidOperationException("Invalid Operations While Fetching Goals Data.");
            }
        }

        public async Task<IEnumerable<T>> GetAllGoalsWithStatusByTaskId<T>(string userId, int taskId, Status status, ResponseDataMode mode) where T : class
        {
             var userIdParam = new SqlParameter("@UserId", userId);
            var statusParam = new SqlParameter("@Status", status);
            var taskIdParam = new SqlParameter("@TaskId", taskId);
            var modeParam = new SqlParameter("@Mode", mode);

            return mode switch
            {
                ResponseDataMode.Model => await _context.Goals
                                             .FromSqlRaw("EXEC usp_GetAllGoalsWithStatusByTaskId @UserId, @Status, @TaskId, @Mode", userIdParam, statusParam, taskIdParam, modeParam)
                                             .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                ResponseDataMode.ModelDTO => await _context.GoalDTOs
                                                .FromSqlRaw("EXEC usp_GetAllGoalsWithStatusByTaskId @UserId, @Status, @TaskId, @Mode", userIdParam, statusParam, taskIdParam, modeParam)
                                                .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                ResponseDataMode.ModelNameDTO => await _context.GoalNameDTOs
                                                    .FromSqlRaw("EXEC usp_GetAllGoalsWithStatusByTaskId @UserId, @Status, @TaskId, @Mode", userIdParam, statusParam, taskIdParam, modeParam)
                                                    .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                _ => throw new InvalidOperationException("Invalid Operations While Fetching Goals Data.")
            };
        }

        public async Task AddGoalAsync(string UserId, Goals goal)
        {
            if(goal.UserId != UserId) throw new ArgumentException("UserId and Goals.UserId must be same!.");
            await _context.Goals.AddAsync(goal);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGoalAsync(string UserID, int Id)
        {
            var goal = await _context.Goals.FirstOrDefaultAsync(g => g.Id == Id && g.User.Id == UserID)
                    ?? throw new NotFoundException($"Goal with ID {Id} not found or does not belong to user {UserID}.");
            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGoalAsync(string UserId, Goals goals)
        {
            var userId = goals.UserId ?? throw new ArgumentNullException("UserId Is Required!.");
            if (userId != UserId) throw new ArgumentException("UserId and Goal.User.Id must be same!.");
            _context.Goals.Update(goals);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGoalStatusAsync(string userId, int goalId, Status statusToChange)
        {
            var userIdParam = new SqlParameter("@UserId", userId);
            var goalIdParam = new SqlParameter("@GoalId", goalId);
            var statusToChangeParam = new SqlParameter("@StatusToUpdate", statusToChange);
            await _context.InsertUpdateSpWithIdDTO.FromSqlRaw("EXEC usp_ChangeGoalStatus @UserId, @GoalId, @StatusToUpdate", userIdParam, goalIdParam, statusToChangeParam).ToListAsync();
        }

        public async Task<int> GetGoalCountByGoalStatus(string UserId, Status status)
        {
            return await _context.Goals.CountAsync(t => t.UserId == UserId && t.GoalStatus == status && t.IsDeleted == false);
        }

        public async Task<int> GetGoalCountByGoalStatusAndDateTimeRange(string UserId, Status status, DateTime from, DateTime end)
        {
            if (from > end) throw new InvalidOperationException("Invalid DateTime Range!");
            return await _context.Goals.CountAsync(g => g.User.Id == UserId && g.GoalStatus == status && g.UpdatedOn >= from && g.UpdatedOn <= end);
        }

        public async Task<IEnumerable<GoalNameDTO>> GetGoalNameBySearchQueryAsync(string userId, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(searchQuery)) return new List<GoalNameDTO>();

            return await _context.Goals
                .Where(g => g.User.Id == userId &&
                    g.DeletedOn == null &&
                    g.EndDate > DateTime.Now &&
                    (g.GoalStatus == Status.Running || g.GoalStatus == Status.NotStarted) &&
                    EF.Functions.Like(g.Name, $"%{searchQuery}%"))
                .Select(g => new GoalNameDTO() { Id = g.Id, Name = g.Name })
                .ToListAsync();
        }

        public async Task<IEnumerable<GoalDTO>> GetRootGoalsAsync(string userId, Status status)
        {
            var query = _context.Goals
                .Where(g => g.UserId == userId && g.IsDeleted == false && g.ParentId == null);

            if (status != Status.All)
            {
                query = query.Where(g => g.GoalStatus == status);
            }

            return await query.Select(g => new GoalDTO
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Priority = g.Priority,
                EndDate = g.EndDate,
                StartDate = g.StartDate,
                GoalStatus = g.GoalStatus,
                SubGoalsCount = g.SubGoals.Count(sg => !sg.IsDeleted)
            }).ToListAsync();
        }

        public async Task<IEnumerable<GoalDTO>> GetChildGoalsAsync(string userId, int parentId)
        {
            return await _context.Goals
                .Where(g => g.UserId == userId && g.IsDeleted == false && g.ParentId == parentId)
                .Select(g => new GoalDTO
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Priority = g.Priority,
                    EndDate = g.EndDate,
                    StartDate = g.StartDate,
                    GoalStatus = g.GoalStatus,
                    SubGoalsCount = g.SubGoals.Count(sg => !sg.IsDeleted),
                    ParentId = g.ParentId,
                    ParentName = g.Parent.Name
                }).ToListAsync();
        }
    }
}
