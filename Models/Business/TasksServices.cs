using AutoMapper;
using Microsoft.Identity.Client;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Models.Business
{
    public class TasksServices(ITaskRepository taskRepository, IGoalRepository goalRepository, IMapper mapper) : ITaskServices
    {
        private readonly ITaskRepository _taskRepository = taskRepository;
        private readonly IGoalRepository _goalRepository = goalRepository;
        private readonly IMapper _mapper = mapper;

        public async Task AddNewTask(string userId, TaskDTO task, string goalIds)
        {
            var UserId = task.UserId ?? throw new ArgumentNullException(nameof(task.UserId), "UserId Is Required!.");

            if (userId != UserId)
            {
                throw new ArgumentException("UserId and Goals.UserId must be same!.");
            }

            task.EndDate = task.EndDate?.AddDays(1).Date.AddMinutes(-1); //Adding the mid-night ending

            if (DateTime.Now > task.EndDate)
            {
                throw new ArgumentException("The end date must be in the future.", nameof(task.EndDate));
            }

            if (task.StartDate >= task.EndDate)
            {
                throw new ArgumentException("Oops! The end date cannot be before or the same as the start date. Please select a later date.", nameof(task.EndDate));
            }

            await _taskRepository.AddTasksAsync(userId, task, goalIds);
        }

        public async Task DeleteTask(string userId, int Id)
        {
            // Implement the soft delete 
            var taskToDelete = await _taskRepository.GetTasksByIdAsync<Tasks>(userId, Id, ResponseDataMode.Model);

            taskToDelete.DeletedOn = DateTime.Now;
            taskToDelete.IsDeleted = true;

            await _taskRepository.UpdateTasksAsync(userId, taskToDelete);
        }

        public async Task<IEnumerable<TaskDTO>> GetAllTasksWithStatusByGoalId(string userId, int goalId, Status taskStatus)
        {
            return await _taskRepository.GetAllTasksWithStatusByGoalId<TaskDTO>(userId, goalId, taskStatus, ResponseDataMode.ModelDTO);
        }

        public async Task<TaskDTOWithGoalListDTO> GetAllGoalsWithStatusAndTask(string userId, int taskId, Status goalStatus)
        {
            var task = await _taskRepository.GetTasksByIdAsync<TaskDTO>(userId, taskId, ResponseDataMode.ModelDTO);
            var goal = await _goalRepository.GetAllGoalsWithStatusByTaskId<GoalDTO>(userId, taskId, goalStatus, ResponseDataMode.ModelDTO);

            var taskWithGoals = _mapper.Map<TaskDTOWithGoalListDTO>(task);
            taskWithGoals.GoalLists = goal;

            return taskWithGoals;
        }

        public async Task<TaskDTOWithGoalNameListDTO> GetAllGoalNamesWithStatusAndTask(string userId, int taskId, Status goalStatus)
        {
            var task = await _taskRepository.GetTasksByIdAsync<TaskDTO>(userId, taskId, ResponseDataMode.ModelDTO);
            var goals = await _goalRepository.GetAllGoalsWithStatusByTaskId<GoalNameDTO>(userId, taskId, goalStatus, ResponseDataMode.ModelNameDTO);

            var taskWithGoals = _mapper.Map<TaskDTOWithGoalNameListDTO>(task);
            taskWithGoals.GoalLists = goals;

            return taskWithGoals;
        }

        public async Task<IEnumerable<TaskDTO>> GetAllTasks(string userId, Status status)
        {
            return await _taskRepository.GetAllTasksAsync<TaskDTO>(userId, status, ResponseDataMode.ModelDTO);
        }

        public async Task<TaskDTO> GetTaskById(string userId, int Id)
        {
            return await _taskRepository.GetTasksByIdAsync<TaskDTO>(userId, Id, ResponseDataMode.ModelDTO);
        }

        public async Task UpdateTask(string userId, TaskDTO task, string goalIds)
        {
            var UserId = task.UserId ?? throw new ArgumentNullException(nameof(task.UserId), "UserId Is Required!.");

            if (userId != UserId)
            {
                throw new ArgumentException("UserId and Goals.UserId must be same!.");
            }

            if (DateTime.Now > task.EndDate)
            {
                throw new ArgumentException("The end date must be in the future.", nameof(task.EndDate));
            }

            var oldTask = await _taskRepository.GetTasksByIdAsync<Tasks>(userId, task.Id, ResponseDataMode.Model);

            if (task.EndDate != oldTask.EndDate)
            {
                task.EndDate = task.EndDate?.AddDays(1).Date.AddMinutes(-1);
            }

            if (task.StartDate >= task.EndDate)
            {
                throw new ArgumentException("Oops! The end date cannot be before or the same as the start date. Please select a later date.", nameof(task.StartDate));
            }

            // Do not re-Scheduled if started 
            if (oldTask.IsStarted)
            {
                task.IsScheduled = oldTask.IsScheduled;
                task.IsStarted = oldTask.IsStarted;
                task.StartDate = oldTask.StartDate;
                task.StartOptionType = oldTask.StartOptionType;
            }

            _mapper.Map<TaskDTO, Tasks>(task, oldTask);

            // if the goal is scheduled and the start date is less than the current date then the goal is started...
            if (oldTask.IsScheduled && oldTask.StartDate <= DateTime.Now && !oldTask.IsStarted)
            {
                oldTask.IsStarted = true;
                oldTask.StartedOn = DateTime.Now;
                oldTask.TaskStatus = Status.Running;
            }


            await _taskRepository.UpdateTasksAsync(userId, oldTask, goalIds);
        }

        public async Task UpdateTask(string userId, TaskDTO task)
        {
            var UserId = task.UserId ?? throw new ArgumentNullException(nameof(task.UserId), "UserId Is Required!.");

            if (userId != UserId)
            {
                throw new ArgumentException("UserId and Goals.UserId must be same!.");
            }

            if (DateTime.Now > task.EndDate)
            {
                throw new ArgumentException("The end date must be in the future.", nameof(task.EndDate));
            }

            var oldTask = await _taskRepository.GetTasksByIdAsync<Tasks>(userId, task.Id, ResponseDataMode.Model);

            if (task.EndDate != oldTask.EndDate)
            {
                task.EndDate = task.EndDate?.AddDays(1).Date.AddMinutes(-1);
            }

            if (task.StartDate >= task.EndDate)
            {
                throw new ArgumentException("Oops! The end date cannot be before or the same as the start date. Please select a later date.", nameof(task.StartDate));
            }

            // Do not re-Scheduled if started 
            if (oldTask.IsStarted)
            {
                task.IsScheduled = oldTask.IsScheduled;
                task.IsStarted = oldTask.IsStarted;
                task.StartDate = oldTask.StartDate;
                task.StartOptionType = oldTask.StartOptionType;
            }

            _mapper.Map<TaskDTO, Tasks>(task, oldTask);

            if (oldTask.IsScheduled && oldTask.StartDate <= DateTime.Now && !oldTask.IsStarted)
            {
                oldTask.IsStarted = true;
                oldTask.StartedOn = DateTime.Now;
                oldTask.TaskStatus = Status.Running;
            }

            await _taskRepository.UpdateTasksAsync(userId, oldTask, String.Empty);
        }

        public async Task UpdateTaskStatus(string userId, int taskId, Status statusToChange)
        {
            await _taskRepository.UpdateTaskStatusAsync(userId, taskId, statusToChange);
        }

        public async Task<int> GetTaskCountByTaskStatus(string userId, Status status)
        {
            return await _taskRepository.GetTaskCountByTaskStatus(userId, status);
        }

        public async Task<int> GetTaskCountByTaskStatusAndDateTimeRange(string userId, Status status, DateTime from, DateTime end)
        {
            return await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, status, from, end);
        }

        public async Task<int> GetTaskCountByTaskStatus(string userId, int goalId, Status status)
        {
            return await _taskRepository.GetTaskCountByTaskStatus(userId, status);
        }

        public async Task<int> GetTaskCountByTaskStatusAndDateTimeRange(string userId, int goalId, Status status, DateTime from, DateTime end)
        {
            return await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, status, from, end);
        }

        public async Task<TaskProductivityDTO> GetTaskProductivity(string userId)
        {
            int runningTask = await _taskRepository.GetTaskCountByTaskStatus(userId, Status.Running);

            // Overall productivity...
            int endTaskCount = await _taskRepository.GetTaskCountByTaskStatus(userId, Status.Ended);
            int completedTaskCount = await _taskRepository.GetTaskCountByTaskStatus(userId, Status.Completed);

            double productivity = 0;
            int totalOverall = endTaskCount + completedTaskCount;
            if (totalOverall > 0)
            {
                productivity = ((double)completedTaskCount / totalOverall) * 100;
            }
            productivity = Math.Round(productivity, 2);


            // Previous week productivity...
            int endTaskPreviousWeekCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, Status.Ended, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));
            int completedPreviousWeekTaskCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, Status.Completed, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));

            double productivityPreviousWeek = 0;
            int totalPreviousWeek = endTaskPreviousWeekCount + completedPreviousWeekTaskCount;
            if (totalPreviousWeek > 0)
            {
                productivityPreviousWeek = ((double)completedPreviousWeekTaskCount / totalPreviousWeek) * 100;
            }
            productivityPreviousWeek = Math.Round(productivityPreviousWeek, 2);


            // Current week productivity...
            int endTaskCurrentWeekCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, Status.Ended, DateTime.Now.AddDays(-7), DateTime.Now);
            int completedCurrentWeekTaskCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, Status.Completed, DateTime.Now.AddDays(-7), DateTime.Now);

            double productivityCurrent = 0;
            int totalCurrentWeek = endTaskCurrentWeekCount + completedCurrentWeekTaskCount;
            if (totalCurrentWeek > 0)
            {
                productivityCurrent = ((double)completedCurrentWeekTaskCount / totalCurrentWeek) * 100;
            }
            productivityCurrent = Math.Round(productivityCurrent, 2);


            // Growth percentage...
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

            TaskProductivityDTO taskProductivity = new TaskProductivityDTO(productivity, runningTask, completedTaskCount, growthPercentage);

            return taskProductivity;
        }


        public async Task<TaskProductivityDTO> GetTaskProductivity(string userId, int goalId)
        {
            int runningTask = await _taskRepository.GetTaskCountByTaskStatus(userId, goalId, Status.Running);

            // Overall productivity...
            int endTaskCount = await _taskRepository.GetTaskCountByTaskStatus(userId, goalId, Status.Ended);
            int completedTaskCount = await _taskRepository.GetTaskCountByTaskStatus(userId, goalId, Status.Completed);

            double productivity = 0;
            int totalOverall = endTaskCount + completedTaskCount;
            if (totalOverall > 0)
            {
                productivity = ((double)completedTaskCount / totalOverall) * 100;
            }
            productivity = Math.Round(productivity, 2);


            // Previous week productivity...
            int endTaskPreviousWeekCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, goalId, Status.Ended, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));
            int completedPreviousWeekTaskCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, goalId, Status.Completed, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));

            double productivityPreviousWeek = 0;
            int totalPreviousWeek = endTaskPreviousWeekCount + completedPreviousWeekTaskCount;
            if (totalPreviousWeek > 0)
            {
                productivityPreviousWeek = ((double)completedPreviousWeekTaskCount / totalPreviousWeek) * 100;
            }
            productivityPreviousWeek = Math.Round(productivityPreviousWeek, 2);


            // Current week productivity...
            int endTaskCurrentWeekCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, goalId, Status.Ended, DateTime.Now.AddDays(-7), DateTime.Now);
            int completedCurrentWeekTaskCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, goalId, Status.Completed, DateTime.Now.AddDays(-7), DateTime.Now);

            double productivityCurrent = 0;
            int totalCurrentWeek = endTaskCurrentWeekCount + completedCurrentWeekTaskCount;
            if (totalCurrentWeek > 0)
            {
                productivityCurrent = ((double)completedCurrentWeekTaskCount / totalCurrentWeek) * 100;
            }
            productivityCurrent = Math.Round(productivityCurrent, 2);


            // Growth percentage...
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

            TaskProductivityDTO taskProductivity = new TaskProductivityDTO(productivity, runningTask, completedTaskCount, growthPercentage);

            return taskProductivity;
        }

        public async Task<IEnumerable<TaskNameDTO>> SearchTasks(string userId, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));
            }

            return await _taskRepository.SearchTasks(userId, query);
        }

        public async Task<IEnumerable<TaskDTOWithGoalNameListDTO>> SearchTasksWithGoals(string userId, string query, Status status)
        {
            return await _taskRepository.SearchTasksWithGoals(userId, query, status);
        }
    }
}
