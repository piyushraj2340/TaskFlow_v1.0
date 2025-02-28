using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
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


            _mapper.Map(goals, findAndUpdateGoal);

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

        public async Task<GoalProductivityDTO> GetGoalProductivity(string UserId)
        {
            int runningGoal = await _goalRepository.GetGoalCountByGoalStatus(UserId, Status.Running);

            // over all productivity...
            int endGoalCount = await _goalRepository.GetGoalCountByGoalStatus(UserId, Status.Ended);
            int completedGoalCount = await _goalRepository.GetGoalCountByGoalStatus(UserId, Status.Completed);

            // Ensure that the division happens with floating-point precision
            double productivity = (((double)completedGoalCount / (endGoalCount + completedGoalCount)) * 100);

            // Round to 2 decimal places
            productivity = Math.Round(productivity, 2);




            // Calculating the previous week productivity....
            int endGoalPreviousWeekCount = await _goalRepository.GetGoalCountByGoalStatusAndDateTimeRange(UserId, Status.Ended, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));
            int completedPreviousWeekGoalCount = await _goalRepository.GetGoalCountByGoalStatusAndDateTimeRange(UserId, Status.Completed, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));

            // Ensure that the division happens with floating-point precision
            double productivityPreviousWeek = (((double)completedPreviousWeekGoalCount / (endGoalPreviousWeekCount + completedPreviousWeekGoalCount)) * 100);

            // Round to 2 decimal places
            productivityPreviousWeek = Math.Round(productivityPreviousWeek, 2);

            // if the completed and ended task is zero then 0 productivity...
            if (endGoalPreviousWeekCount + completedPreviousWeekGoalCount == 0)
            {
                productivityPreviousWeek = 0;
            }


            // Calculating the current week productivity....
            int endGoalCurrentWeekCount = await _goalRepository.GetGoalCountByGoalStatusAndDateTimeRange(UserId, Status.Ended, DateTime.Now.AddDays(-7), DateTime.Now);
            int completedCurrentWeekGoalCount = await _goalRepository.GetGoalCountByGoalStatusAndDateTimeRange(UserId, Status.Completed, DateTime.Now.AddDays(-7), DateTime.Now);

            // Ensure that the division happens with floating-point precision
            double productivityCurrent = (((double)completedCurrentWeekGoalCount / (endGoalCurrentWeekCount + completedCurrentWeekGoalCount)) * 100);

            // Round to 2 decimal places
            productivityCurrent = Math.Round(productivityCurrent, 2);

            // if the completed and ended task is zero then 0 productivity...
            if (endGoalCurrentWeekCount + completedCurrentWeekGoalCount == 0)
            {
                productivityCurrent = 0;
            }

            double growthPercentage = ((double)((productivityCurrent - productivityPreviousWeek) / productivityPreviousWeek) * 100);


            // Round to 2 decimal places
            growthPercentage = Math.Round(growthPercentage, 2);


            // if the previous groth percentage is 0 then 100% groth..
            if (productivityPreviousWeek == 0)
            {
                growthPercentage = 100.00d;
            }

            GoalProductivityDTO goalProductivity = new(productivity, runningGoal, completedGoalCount, growthPercentage);

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
    }
}
