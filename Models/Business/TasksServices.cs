using AutoMapper;
using Microsoft.Identity.Client;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
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
            return await _taskRepository.GetAllTasksWithStatusByGoalId<TaskDTO>(userId,goalId, taskStatus, ResponseDataMode.ModelDTO);
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
            var task = await _taskRepository.GetTasksByIdAsync<TaskDTO>(userId, taskId,ResponseDataMode.ModelDTO);
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

            _mapper.Map<TaskDTO, Tasks>(task, oldTask);

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

            _mapper.Map<TaskDTO, Tasks>(task, oldTask);

            await _taskRepository.UpdateTasksAsync(userId, oldTask, String.Empty);
        }

        public async Task UpdateTaskStatus(string userId, int taskId, Status statusToChange)
        {
            await _taskRepository.UpdateTaskStatusAsync(userId,taskId, statusToChange);
        }

        public async Task<int> GetTaskCountByTaskStatus(string userId, Status status)
        {
            return await _taskRepository.GetTaskCountByTaskStatus(userId, status);
        }

        public async Task<int> GetTaskCountByTaskStatusAndDateTimeRange(string userId, Status status, DateTime from, DateTime end)
        {
            return await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, status, from, end);
        }

        public async Task<TaskProductivityDTO> GetTaskProductivity(string userId)
        {
            int runningTask = await _taskRepository.GetTaskCountByTaskStatus(userId, Status.Running);

            // over all productivity...
            int endTaskCount = await _taskRepository.GetTaskCountByTaskStatus(userId, Status.Ended);
            int completedTaskCount = await _taskRepository.GetTaskCountByTaskStatus(userId, Status.Completed);

            // Ensure that the division happens with floating-point precision
            double productivity = (((double)completedTaskCount / (endTaskCount + completedTaskCount)) * 100);

            // Round to 2 decimal places
            productivity = Math.Round(productivity, 2);




            // Calculating the previous week productivity....
            int endTaskPreviousWeekCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, Status.Ended, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));
            int completedPreviousWeekTaskCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, Status.Completed, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));

            // Ensure that the division happens with floating-point precision
            double productivityPreviousWeek = (((double)completedPreviousWeekTaskCount / (endTaskPreviousWeekCount + completedPreviousWeekTaskCount)) * 100);

            // Round to 2 decimal places
            productivityPreviousWeek = Math.Round(productivityPreviousWeek, 2);

            // if the completed and ended task is zero then 0 productivity...
            if (endTaskPreviousWeekCount + completedPreviousWeekTaskCount == 0)
            {
                productivityPreviousWeek = 0;
            }


            // Calculating the current week productivity....
            int endTaskCurrentWeekCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, Status.Ended, DateTime.Now.AddDays(-7), DateTime.Now);
            int completedCurrentWeekTaskCount = await _taskRepository.GetTaskCountByTaskStatusAndDateTimeRange(userId, Status.Completed, DateTime.Now.AddDays(-7), DateTime.Now);

            // Ensure that the division happens with floating-point precision
            double productivityCurrent = (((double)completedCurrentWeekTaskCount / (endTaskCurrentWeekCount + completedCurrentWeekTaskCount)) * 100);

            // Round to 2 decimal places
            productivityCurrent = Math.Round(productivityCurrent, 2);

            // if the completed and ended task is zero then 0 productivity...
            if (endTaskCurrentWeekCount + completedCurrentWeekTaskCount == 0)
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

            TaskProductivityDTO taskProductivity = new TaskProductivityDTO(productivity, runningTask, completedTaskCount, growthPercentage);

            return taskProductivity;
        }
    }
}
