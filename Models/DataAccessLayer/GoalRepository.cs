using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Data.Common;
using TaskMonitoringApp.Exceptions;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Repositories;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class GoalRepository(ApplicationDbContext context) : IGoalRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<T>> GetAllGoalsAsync<T>(string UserId, Status status, ResponseDataMode mode) where T : class
        {
            var baseQuery = _context.Goals.Where(g => g.UserId == UserId && g.IsDeleted == false);

            if (status != Status.All)
            {
                baseQuery = baseQuery.Where(g => g.GoalStatus == status);
            }

            return mode switch
            {
                ResponseDataMode.Model => await baseQuery
                                            .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                ResponseDataMode.ModelDTO => await baseQuery
                                                .Where(g => g.ParentId == null)
                                                .Select(g => new GoalDTO
                                                {
                                                    Id = g.Id,
                                                    Name = g.Name,
                                                    EndDate = g.EndDate,
                                                    Priority = g.Priority,
                                                    Description = g.Description,
                                                    GoalStatus = g.GoalStatus,
                                                    UserId = g.UserId,
                                                    IsScheduled = g.IsScheduled,
                                                    StartDate = g.StartDate,
                                                    IsStarted = g.IsStarted,
                                                    StartOptionType = g.StartOptionType,
                                                    ParentId = g.ParentId,
                                                    ParentName = "",
                                                    SubGoalsCount = _context.Goals.Count(sg => sg.ParentId == g.Id && sg.IsDeleted == false)
                                                })
                                                .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                ResponseDataMode.ModelNameDTO => await baseQuery
                                                    .Select(g => new GoalNameDTO
                                                    {
                                                        Id = g.Id,
                                                        Name = g.Name,
                                                        UserId = g.UserId
                                                    })
                                                    .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                _ => throw new InvalidOperationException("Invalid Operations While Fetching Goals Data.")
            };
        }

        public async Task<T> GetGoalByIdAsync<T>(string userId, int id, ResponseDataMode mode) where T : class
        {
            var baseQuery = _context.Goals.Where(g => g.UserId == userId && g.Id == id && g.IsDeleted == false);

            switch (mode)
            {
                case ResponseDataMode.Model:
                    
                    var dataGoalModel = await baseQuery.FirstOrDefaultAsync();

                    return dataGoalModel as T
                            ?? throw new NotFoundException($"Goal with ID {id} not found or does not belong to user {userId}.");

                case ResponseDataMode.ModelDTO:

                    var dataGoalDTO = await baseQuery
                        .Select(g => new GoalDTO
                        {
                            Id = g.Id,
                            Name = g.Name,
                            Description = g.Description,
                            Priority = g.Priority,
                            GoalStatus = g.GoalStatus,
                            StartDate = g.StartDate,
                            EndDate = g.EndDate,
                            IsScheduled = g.IsScheduled,
                            IsStarted = g.IsStarted,
                            StartOptionType = g.StartOptionType,
                            UserId = g.UserId,
                            ParentId = g.ParentId,
                            ParentName = g.Parent != null ? g.Parent.Name : null,
                            SubGoalsCount = _context.Goals.Count(sg => sg.ParentId == g.Id && sg.IsDeleted == false)
                        }).FirstOrDefaultAsync();

                    return dataGoalDTO as T
                            ?? throw new NotFoundException($"Goal with ID {id} not found or does not belong to user {userId}.");

                case ResponseDataMode.ModelNameDTO:

                    var dataGoalNameDTO = await baseQuery
                        .Select(g => new GoalNameDTO
                        {
                            Id = g.Id,
                            Name = g.Name,
                            UserId = g.UserId,
                            GoalStatus = g.GoalStatus
                        }).FirstOrDefaultAsync();

                    return dataGoalNameDTO as T
                            ?? throw new NotFoundException($"Goal with ID {id} not found or does not belong to user {userId}.");

                default: throw new InvalidOperationException("Invalid Operations While Fetching Goals Data.");
            }
        }

        public async Task<IEnumerable<T>> GetAllGoalsWithStatusByTaskId<T>(string userId, int taskId, Status status, ResponseDataMode mode) where T : class
        {
            var currentDateTime = DateTime.Now;

            var baseQuery = _context.GoalTasks
                .Where(gt => gt.TaskId == taskId && gt.UserId == userId && gt.Goal.UserId == userId && gt.Goal.IsDeleted == false && gt.Goal.EndDate > currentDateTime);

            if (status != Status.All)
            {
                baseQuery = baseQuery.Where(gt => gt.Goal.GoalStatus == status);
            }

            return mode switch
            {
                ResponseDataMode.Model => await baseQuery
                                            .Select(gt => gt.Goal)
                                            .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                ResponseDataMode.ModelDTO => await baseQuery
                                                .Select(gt => new GoalDTO
                                                {
                                                    Id = gt.Goal.Id,
                                                    Name = gt.Goal.Name,
                                                    EndDate = gt.Goal.EndDate,
                                                    Priority = gt.Goal.Priority,
                                                    Description = gt.Goal.Description,
                                                    GoalStatus = gt.Goal.GoalStatus,
                                                    UserId = gt.Goal.UserId,
                                                    IsScheduled = gt.Goal.IsScheduled,
                                                    StartDate = gt.Goal.StartDate,
                                                    IsStarted = gt.Goal.IsStarted,
                                                    StartOptionType = gt.Goal.StartOptionType
                                                })
                                                .ToListAsync() as IEnumerable<T> ?? throw new NotFoundException("Goals Data Not Found!"),

                ResponseDataMode.ModelNameDTO => await baseQuery
                                                .Select(gt => new GoalNameDTO
                                                {
                                                    Id = gt.Goal.Id,
                                                    Name = gt.Goal.Name,
                                                    UserId = gt.Goal.UserId,
                                                    GoalStatus = gt.Goal.GoalStatus
                                                })
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
            var currentDateTime = DateTime.Now;

            var goal = await _context.Goals
                .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId && g.IsDeleted == false && g.EndDate > currentDateTime);

            if (goal == null)
            {
                throw new NotFoundException("Goal Not Found! Goal Must be in Active State");
            }

            var currentGoalStatus = goal.GoalStatus;

            if (statusToChange == Status.NotStarted)
            {
                if (currentGoalStatus == Status.Running)
                {
                    goal.GoalStatus = Status.NotStarted;
                    goal.UpdatedOn = currentDateTime;
                }
                else
                {
                    throw new InvalidOperationException("Goal must be in running state!");
                }
            }
            else if (statusToChange == Status.Running)
            {
                if (currentGoalStatus == Status.NotStarted || currentGoalStatus == Status.Completed || currentGoalStatus == Status.Ended)
                {
                    goal.GoalStatus = Status.Running;
                    goal.UpdatedOn = currentDateTime;

                    if (currentGoalStatus == Status.NotStarted && !goal.IsStarted)
                    {
                        goal.IsStarted = true;
                        goal.StartedOn = currentDateTime;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Goal must be in Active State!");
                }
            }
            else if (statusToChange == Status.Completed)
            {
                if (currentGoalStatus == Status.Running)
                {
                    goal.GoalStatus = Status.Completed;
                    goal.UpdatedOn = currentDateTime;
                    goal.CompletedOn = currentDateTime;
                }
                else
                {
                    throw new InvalidOperationException("Goal must be in running state!");
                }
            }
            else if (statusToChange == Status.Ended)
            {
                if (currentGoalStatus == Status.Running)
                {
                    goal.GoalStatus = Status.Ended;
                    goal.UpdatedOn = currentDateTime;
                    goal.EndedOn = currentDateTime;
                }
                else
                {
                    throw new InvalidOperationException("Goal must be in running state!");
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid Goal Status!");
            }

            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();
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

        public async Task UpdateAutoStartedGoalsAsync(string userId)
        {
            var currentDateTime = DateTime.Now;

            var goalsToUpdate = await _context.Goals
                .Where(g => g.UserId == userId 
                         && g.IsScheduled 
                         && !g.IsStarted 
                         && g.StartDate < currentDateTime 
                         && g.GoalStatus == Status.NotStarted 
                         && !g.IsDeleted)
                .ToListAsync();

            if (goalsToUpdate.Any())
            {
                foreach (var goal in goalsToUpdate)
                {
                    goal.GoalStatus = Status.Running;
                    goal.UpdatedOn = currentDateTime;
                    goal.IsStarted = true;
                    goal.StartedOn = currentDateTime;
                }

                _context.Goals.UpdateRange(goalsToUpdate);
                await _context.SaveChangesAsync();
            }
        }
    }
}
