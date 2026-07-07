using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;
using TaskMonitoringApp.Models.ViewModel;

namespace TaskMonitoringApp.Models.Business
{
    public class GoalServices(ITaskRepository taskRepository, IGoalRepository goalRepository, IMapper mapper) : IGoalServices
    {
        private readonly ITaskRepository _taskRepository = taskRepository;
        private readonly IGoalRepository _goalRepository = goalRepository;
        private readonly IMapper _mapper = mapper;

        public async Task AddNewGoal(string UserId, GoalDTO goals)
        {
            if (goals.Id == goals.ParentId)
            {
                throw new ArgumentException("ParentId and GoalId not be same.");
            }

            var userId = goals.UserId ?? throw new ArgumentNullException(nameof(goals.UserId), "UserId Is Required!.");

            if (userId != UserId)
            {
                throw new ArgumentException("UserId and Goals.UserId must be same!.");
            }

            goals.EndDate = goals.EndDate.AddDays(1).Date.AddMinutes(-1); //Adding the mid-night ending

            if (DateTime.Now > goals.EndDate)
            {
                throw new ArgumentException("The end date must be in the future.", nameof(goals.EndDate));
            }

            var goalToAdd = _mapper.Map<Goals>(goals);

            if (goals.IsScheduled && goals.StartDate <= DateTime.Now)
            {
                goalToAdd.IsStarted = true;
                goalToAdd.StartedOn = DateTime.Now;
                goalToAdd.GoalStatus = Status.Running;
            }

            if (goalToAdd.StartDate >= goalToAdd.EndDate)
            {
                throw new ArgumentException("Oops! The end date cannot be before or the same as the start date. Please select a later date.", nameof(goals.EndDate));
            }


            if (goals.ParentId.HasValue && goals.ParentId > 0)
            {
                if (goals.ParentId == goals.Id)
                {
                    throw new ArgumentException("A goal cannot be its own parent. Please select a different parent goal.", nameof(goals.ParentId));
                }
                // Check for circular reference
                var parentGoal = await _goalRepository.GetGoalByIdAsync<Goals>(UserId, goals.ParentId.Value, ResponseDataMode.Model);
                while (parentGoal != null)
                {
                    if (parentGoal.ParentId == goals.Id)
                    {
                        throw new ArgumentException("Circular reference detected. A goal cannot be a parent of its own descendant.", nameof(goals.ParentId));
                    }
                    if (parentGoal.ParentId.HasValue)
                    {
                        parentGoal = await _goalRepository.GetGoalByIdAsync<Goals>(UserId, parentGoal.ParentId.Value, ResponseDataMode.Model);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            goalToAdd.Parent = null;

            // ParentId is mapped automatically by AutoMapper if names match
            await _goalRepository.AddGoalAsync(UserId, goalToAdd);
        }

        public async Task DeleteGoal(string UserId, int GoalId)
        {
            // Implemented the soft Delete....
            var getGoalAndDelete = await _goalRepository.GetGoalByIdAsync<Goals>(UserId, GoalId, ResponseDataMode.Model);

            getGoalAndDelete.DeletedOn = DateTime.Now;
            getGoalAndDelete.IsDeleted = true;

            await _goalRepository.UpdateGoalAsync(UserId, getGoalAndDelete);
        }

        public async Task<IEnumerable<GoalDTO>> GetAllGoals(string UserId, Status status)
        {
            return await _goalRepository.GetAllGoalsAsync<GoalDTO>(UserId, status, ResponseDataMode.ModelDTO);
        }

        public async Task<GoalDTO> GetGoalById(string UserId, int Id)
        {
            return await _goalRepository.GetGoalByIdAsync<GoalDTO>(UserId, Id, ResponseDataMode.ModelDTO);
        }

        public async Task UpdateGoal(string UserId, GoalDTO goals)
        {
            if(goals.Id == goals.ParentId)
            {
                throw new ArgumentException("ParentId and GoalId not be same.");
            }
            if (DateTime.Now > goals.EndDate)
            {
                throw new ArgumentException("The end date must be in the future.");
            }

            var findAndUpdateGoal = await _goalRepository.GetGoalByIdAsync<Goals>(UserId, goals.Id, ResponseDataMode.Model);

            // update the date...
            if (goals.EndDate != findAndUpdateGoal.EndDate)
            {
                goals.EndDate = goals.EndDate.AddDays(1).Date.AddMinutes(-1);
            }

            if (goals.StartDate >= goals.EndDate)
            {
                throw new ArgumentException("Oops! The end date cannot be before or the same as the start date. Please select a later date.", nameof(goals.StartDate));
            }

            // Do not re-Scheduled if started 
            if (findAndUpdateGoal.IsStarted)
            {
                goals.IsScheduled = findAndUpdateGoal.IsScheduled;
                goals.IsStarted = findAndUpdateGoal.IsStarted;
                goals.StartDate = findAndUpdateGoal.StartDate;
                goals.StartOptionType = findAndUpdateGoal.StartOptionType;
            }

            if (goals.ParentId.HasValue && goals.ParentId > 0)
            {
                if (goals.ParentId == goals.Id)
                {
                    throw new ArgumentException("A goal cannot be its own parent. Please select a different parent goal.", nameof(goals.ParentId));
                }
                // Check for circular reference
                var parentGoal = await _goalRepository.GetGoalByIdAsync<Goals>(UserId, goals.ParentId.Value, ResponseDataMode.Model, RequestDataMode.AsNoTracking);
                while (parentGoal != null)
                {
                    if (parentGoal.ParentId == goals.Id)
                    {
                        throw new ArgumentException("Circular reference detected. A goal cannot be a parent of its own descendant.", nameof(goals.ParentId));
                    }
                    if (parentGoal.ParentId.HasValue)
                    {
                        parentGoal = await _goalRepository.GetGoalByIdAsync<Goals>(UserId, parentGoal.ParentId.Value, ResponseDataMode.Model, RequestDataMode.AsNoTracking);
                    }
                    else
                    {
                        break;
                    }
                }
            }


            _mapper.Map(goals, findAndUpdateGoal);

            findAndUpdateGoal.Parent = null;

            // if the goal is scheduled and the start date is less than the current date then the goal is started...
            if (findAndUpdateGoal.IsScheduled && findAndUpdateGoal.StartDate <= DateTime.Now && !findAndUpdateGoal.IsStarted)
            {
                findAndUpdateGoal.IsStarted = true;
                findAndUpdateGoal.StartedOn = DateTime.Now;
                findAndUpdateGoal.GoalStatus = Status.Running;
            }

            await _goalRepository.UpdateGoalAsync(UserId, findAndUpdateGoal);
        }

        public async Task UpdateGoalStatus(string userId, int goalId, Status statusToUpdate)
        {
            await _goalRepository.UpdateGoalStatusAsync(userId, goalId, statusToUpdate);
        }

        public async Task<int> GetGoalCountByGoalStatus(string UserId, Status status)
        {
            return await _goalRepository.GetGoalCountByGoalStatus(UserId, status);
        }

        public async Task<int> GetGoalCountByGoalStatusAndDateTimeRange(string UserId, Status status, DateTime from, DateTime end)
        {
            return await _goalRepository.GetGoalCountByGoalStatusAndDateTimeRange(UserId, status, from, end);
        }

        public async Task<GoalProductivityDTO> GetGoalProductivity(string userId)
        {
            // Keep stats accurate dynamically
            
            await _goalRepository.UpdateGoalStateAsync(userId);
            
            int runningGoal = await _goalRepository.GetGoalCountByGoalStatus(userId, Status.Running);

            // Overall productivity
            int endGoalCount = await _goalRepository.GetGoalCountByGoalStatus(userId, Status.Ended);
            int completedGoalCount = await _goalRepository.GetGoalCountByGoalStatus(userId, Status.Completed);

            double productivity = 0;
            int totalOverall = endGoalCount + completedGoalCount;
            if (totalOverall > 0)
            {
                productivity = ((double)completedGoalCount / totalOverall) * 100;
            }
            productivity = Math.Round(productivity, 2);


            // Previous week productivity
            int endGoalPreviousWeekCount = await _goalRepository.GetGoalCountByGoalStatusAndDateTimeRange(
                userId, Status.Ended, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));
            int completedPreviousWeekGoalCount = await _goalRepository.GetGoalCountByGoalStatusAndDateTimeRange(
                userId, Status.Completed, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));

            double productivityPreviousWeek = 0;
            int totalPreviousWeek = endGoalPreviousWeekCount + completedPreviousWeekGoalCount;
            if (totalPreviousWeek > 0)
            {
                productivityPreviousWeek = ((double)completedPreviousWeekGoalCount / totalPreviousWeek) * 100;
            }
            productivityPreviousWeek = Math.Round(productivityPreviousWeek, 2);


            // Current week productivity
            int endGoalCurrentWeekCount = await _goalRepository.GetGoalCountByGoalStatusAndDateTimeRange(
                userId, Status.Ended, DateTime.Now.AddDays(-7), DateTime.Now);
            int completedCurrentWeekGoalCount = await _goalRepository.GetGoalCountByGoalStatusAndDateTimeRange(
                userId, Status.Completed, DateTime.Now.AddDays(-7), DateTime.Now);

            double productivityCurrent = 0;
            int totalCurrentWeek = endGoalCurrentWeekCount + completedCurrentWeekGoalCount;
            if (totalCurrentWeek > 0)
            {
                productivityCurrent = ((double)completedCurrentWeekGoalCount / totalCurrentWeek) * 100;
            }
            productivityCurrent = Math.Round(productivityCurrent, 2);


            // Growth percentage
            double growthPercentage = 0;
            if (productivityPreviousWeek > 0)
            {
                growthPercentage = ((productivityCurrent - productivityPreviousWeek) / productivityPreviousWeek) * 100;
            }
            else if (productivityCurrent > 0)
            {
                growthPercentage = 100.0d;
            }

            growthPercentage = Math.Round(growthPercentage, 2);

            // Handle NaN / Infinity safety
            if (double.IsNaN(productivity)) productivity = 0;
            if (double.IsNaN(productivityPreviousWeek)) productivityPreviousWeek = 0;
            if (double.IsNaN(productivityCurrent)) productivityCurrent = 0;
            if (double.IsNaN(growthPercentage) || double.IsInfinity(growthPercentage)) growthPercentage = 0;

            GoalProductivityDTO goalProductivity = new GoalProductivityDTO(
                productivity,
                runningGoal,
                completedGoalCount,
                growthPercentage
            );

            return goalProductivity;
        }


        public async Task<IEnumerable<GoalNameDTO>> GetGoalNameBySearchQuery(string userId, string searchQuery)
        {
            return await _goalRepository.GetGoalNameBySearchQueryAsync(userId, searchQuery);
        }

        public async Task<GoalDTOWithTaskNameListDTO> GetAllTaskNameWithStatusAndGoal(string userId, int goalId, Status goalStatus)
        {
            var goal = await _goalRepository.GetGoalByIdAsync<GoalDTO>(userId, goalId, ResponseDataMode.ModelDTO);
            var taskNameList = await _taskRepository.GetAllTasksWithStatusByGoalId<TaskNameDTO>(userId, goalId, Status.All, ResponseDataMode.ModelNameDTO);

            var goalNameWithTaskName = _mapper.Map<GoalDTOWithTaskNameListDTO>(goal);
            goalNameWithTaskName.TaskLists = taskNameList;

            return goalNameWithTaskName;
        }

        public async Task<GoalDTOWithTaskListDTO> GetAllTaskWithStatusAndGoal(string userId, int goalId, Status goalStatus)
        {
            var goal = await _goalRepository.GetGoalByIdAsync<GoalDTO>(userId, goalId, ResponseDataMode.ModelDTO);
            var taskNameList = await _taskRepository.GetAllTasksWithStatusByGoalId<TaskDTO>(userId, goalId, Status.All, ResponseDataMode.ModelDTO);

            var goalNameWithTask = _mapper.Map<GoalDTOWithTaskListDTO>(goal);
            goalNameWithTask.TaskLists = taskNameList;

            return goalNameWithTask;
        }


        public async Task<GoalNameDTO> GetGoalNameById(string UserId, int Id)
        {
            return await _goalRepository.GetGoalByIdAsync<GoalNameDTO>(UserId, Id, ResponseDataMode.ModelNameDTO);
        }

        // --- New Implementations ---

        public async Task<IEnumerable<GoalDTO>> GetRootGoals(string userId, Status status)
        {
            // We need to implement this in Repository or use EF directly here if Repository is generic
            // Assuming we add specific methods to IGoalRepository
            return await _goalRepository.GetRootGoalsAsync(userId, status);
        }

        public async Task<IEnumerable<GoalDTO>> GetChildGoals(string userId, int parentId)
        {
            return await _goalRepository.GetChildGoalsAsync(userId, parentId);
        }

        public async Task<IEnumerable<GoalDTO>> GetAllGoalsWithAutoStartAsync(string userId, Status status)
        {
            if (status == Status.Running || status == Status.All)
            {
                
            }
            return await _goalRepository.GetAllGoalsAsync<GoalDTO>(userId, status, ResponseDataMode.ModelDTO);
        }

        public async Task<IEnumerable<GoalDTO>> GetRootGoalsWithAutoStartAsync(string userId, Status status)
        {
            if (status == Status.Running || status == Status.All)
            {
                
            }
            return await _goalRepository.GetRootGoalsAsync(userId, status);
        }

        public async Task<IEnumerable<GoalDTO>> GetAllGoalsWithDynamicStatusUpdatesAsync(string userId, Status status)
        {
            if (status != Status.Completed && status != Status.Archived)
            {
                
                await _goalRepository.UpdateGoalStateAsync(userId);
            }
            return await _goalRepository.GetAllGoalsAsync<GoalDTO>(userId, status, ResponseDataMode.ModelDTO);
        }

        public async Task<IEnumerable<GoalDTO>> GetRootGoalsWithDynamicStatusUpdatesAsync(string userId, Status status)
        {
            if (status != Status.Completed && status != Status.Archived)
            {
                
                await _goalRepository.UpdateGoalStateAsync(userId);
            }
            return await _goalRepository.GetRootGoalsAsync(userId, status);
        }

        public async Task<bool> DeattachSubGoals(string userId, List<int> goalIds)
        {
            if (goalIds == null || !goalIds.Any()) return false;

            await _goalRepository.DeattachSubGoalsAsync(userId, goalIds);
            return true;
        }
    }
}
