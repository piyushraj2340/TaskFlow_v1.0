using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Threading.Tasks;
using TaskMonitoringApp.Models.Business;
using TaskMonitoringApp.Models.DataAccessLayer;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;
using TaskMonitoringApp.Models.ViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskMonitoringApp.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ILogger<TasksController> _logger;
        private readonly IMapper _mapper;
        private readonly ITaskServices _taskService;
        private readonly INotesServices _notesService;
        private readonly IGoalServices _goalsService;
        private readonly UserManager<Users> _userManager;
        private readonly ITodoServices _todoService;

        public TasksController(ILogger<TasksController> logger, IMapper mapper, ITaskServices taskService, INotesServices notesService, IGoalServices goalsService, UserManager<Users> userManager, ITodoServices todoService)
        {
            _logger = logger;
            _mapper = mapper;
            _taskService = taskService;
            _notesService = notesService;
            _goalsService = goalsService;
            _userManager = userManager;
            _todoService = todoService;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Entered Index action.");
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Index.");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("Fetching task productivity for user {UserId}.", userId);
            TaskProductivityDTO productivity = await _taskService.GetTaskProductivity(userId);
            _logger.LogInformation("Fetched productivity for user {UserId}: {@Productivity}", userId, productivity);
            return View(productivity);
        }

        [Route("{controller}/{action}/{Id}/{tabName?}")]
        public async Task<IActionResult> Details(int Id, string? tabName)
        {
            _logger.LogInformation("Entered Details with Id={Id}, tabName={TabName}", Id, tabName);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Details.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var task = await _taskService.GetAllGoalNamesWithStatusAndTask(userId, Id, Status.All);

                if (task == null)
                {
                    _logger.LogWarning("Task not found for Id={Id}, userId={UserId}", Id, userId);
                    return NotFound();
                }

                ViewBag.tabName = tabName;

                var notesList = await _notesService.GetAllNotesByTaskId(userId, Id, Status.All, pageNumber: 1, pageSize: 5);
                var taskWithNoteList = _mapper.Map<TaskViewModel>(task);

                var productivity = await _todoService.GetTodoProgressAnalyses(userId, Id);

                taskWithNoteList.NotesLists = notesList;
                taskWithNoteList.GoalLists = task.GoalLists;
                taskWithNoteList.TodoProgress = productivity;

                _logger.LogInformation("Returning details for task Id={Id}, userId={UserId}", Id, userId);
                return View(taskWithNoteList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Details for Id={Id}, userId={UserId}", Id, userId);
                throw;
            }
        }

        public async Task<IActionResult> Create(int? goalId)
        {
            _logger.LogInformation("Entered Create with goalId={GoalId}", goalId);
            ViewBag.IsEditMode = false;
            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString();

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Create.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (goalId.HasValue)
                {
                    TaskViewModel emptyTask = new TaskViewModel();
                    var goalName = await _goalsService.GetGoalNameById(userId, goalId.Value);

                    emptyTask.GoalIds = goalId.Value.ToString();
                    emptyTask.GoalLists = new[] { goalName };

                    _logger.LogInformation("Returning empty task for goalId={GoalId}, userId={UserId}", goalId, userId);
                    return View(emptyTask);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Create for goalId={GoalId}, userId={UserId}", goalId, userId);
                throw;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,Description,Repeat,RepeatWeekList,Priority,EndDate,TasksList,GoalIds,StartOptionType,StartDate")] TaskViewModel task)
        {
            _logger.LogInformation("Entered Create (POST) with task={@Task}", task);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Create (POST).");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                ViewBag.IsEditMode = false;
                var returnUrl = string.IsNullOrWhiteSpace(TempData["ReturnUrl"]?.ToString()) ? Url.Action("Index", "Home") : TempData["ReturnUrl"]?.ToString();

                // Validations
                if (task.StartOptionType == StartOptions.Scheduled && task.StartDate == null)
                {
                    ModelState.AddModelError("StartDate", "Start Date Must be required for scheduled!");
                    _logger.LogWarning("Start date required for scheduled task creation.");
                }

                if (task.StartDate >= task?.EndDate?.Date)
                {
                    ModelState.AddModelError("StartDate", "Oops! The end date cannot be before or the same as the start date. Please select a later date.");
                    _logger.LogWarning("Invalid start and end date for task creation.");
                }

                if (DateTime.Now > task?.EndDate)
                {
                    ModelState.AddModelError("EndDate", "The end date must be in the future.");
                    _logger.LogWarning("End date must be in the future for task creation.");
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Attempting to add new task: {TaskName}", task?.Name);

                    if (task?.StartOptionType == StartOptions.Immediate)
                    {
                        task.IsScheduled = true;
                        task.StartDate = DateTime.Now;
                    }
                    else if (task?.StartOptionType == StartOptions.Scheduled)
                    {
                        task.IsScheduled = true;
                    }

                    var newTask = _mapper.Map<TaskDTO>(task);
                    newTask.UserId = userId;

                    await _taskService.AddNewTask(userId, newTask, task.GoalIds);

                    _logger.LogInformation("Task '{TaskName}' successfully added with ID: {TaskID}", task.Name, task.Id);
                    return Redirect(returnUrl ?? "/");
                }
            }
            catch (ArgumentException ae)
            {
                _logger.LogWarning(ae, "Validation error in Create (POST) for task={@Task}", task);
                ModelState.AddModelError("", !string.IsNullOrWhiteSpace(ae.Message) ? ae.Message : "Invalid Input Fields");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Create (POST) for task={@Task}", task);
                throw;
            }

            return View(task);
        }

        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Entered Edit with id={Id}", id);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Edit.");
                return RedirectToAction("Login", "Account");
            }

            ViewBag.IsEditMode = true;
            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString();

            try
            {
                var taskToEdit = await _taskService.GetAllGoalNamesWithStatusAndTask(userId, id, Status.All);
                if (taskToEdit == null)
                {
                    _logger.LogWarning("Task not found for id={Id}, userId={UserId}", id, userId);
                    return NotFound();
                }

                TaskViewModel taskViewModel = _mapper.Map<TaskViewModel>(taskToEdit);
                taskViewModel.GoalLists = taskToEdit.GoalLists;

                if (taskViewModel != null && taskViewModel.GoalLists != null)
                {
                    taskViewModel.GoalIds = string.Join(",", taskViewModel.GoalLists.Select(gId => gId.Id));
                }

                _logger.LogInformation("Returning task for edit with id={Id}, userId={UserId}", id, userId);
                return View(taskViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Edit for id={Id}, userId={UserId}", id, userId);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Repeat,TaskStatus,RepeatWeekList,Priority,EndDate,TasksList,GoalIds,StartOptionType,StartDate")] TaskViewModel task)
        {
            _logger.LogInformation("Entered Edit (POST) with id={Id}, task={@Task}", id, task);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Edit (POST).");
                return RedirectToAction("Login", "Account");
            }

            if (id != task.Id)
            {
                _logger.LogWarning("Task ID mismatch in Edit (POST). Expected: {ExpectedId}, Received: {ActualId}", task.Id, id);
                return NotFound();
            }

            task.UserId = userId;

            ViewBag.IsEditMode = true;
            var returnUrl = string.IsNullOrWhiteSpace(TempData["ReturnUrl"]?.ToString()) ? Url.Action("Index", "Home") : TempData["ReturnUrl"]?.ToString();

            // Validations
            if (task.StartOptionType == StartOptions.Scheduled && task.StartDate == null)
            {
                ModelState.AddModelError("StartDate", "Start Date Must be required for scheduled!");
                _logger.LogWarning("Start date required for scheduled task edit.");
            }

            if (task.StartDate >= task?.EndDate?.Date)
            {
                ModelState.AddModelError("StartDate", "Oops! The end date cannot be before or the same as the start date. Please select a later date.");
                _logger.LogWarning("Invalid start and end date for task edit.");
            }

            if (DateTime.Now > task?.EndDate)
            {
                ModelState.AddModelError("EndDate", "The end date must be in the future.");
                _logger.LogWarning("End date must be in the future for task edit.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to update task: {TaskName}", task.Name);

                    if (task.StartOptionType == StartOptions.Immediate)
                    {
                        task.IsScheduled = true;
                        task.StartDate = DateTime.Now;
                    }
                    else if (task.StartOptionType == StartOptions.Scheduled)
                    {
                        task.IsScheduled = true;
                    }
                    else if (task.StartOptionType == StartOptions.Manual)
                    {
                        task.IsScheduled = false;
                    }

                    var taskDto = _mapper.Map<TaskDTO>(task);
                    taskDto.UserId = userId;

                    if (string.IsNullOrWhiteSpace(task.GoalIds))
                    {
                        await _taskService.UpdateTask(userId, taskDto);
                    }
                    else
                    {
                        await _taskService.UpdateTask(userId, taskDto, task.GoalIds);
                    }

                    _logger.LogInformation("Task '{TaskName}' successfully updated with ID: {TaskID}", task.Name, task.Id);
                    return Redirect(returnUrl ?? "/");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception in Edit (POST) for id={Id}, task={@Task}", id, task);
                    throw;
                }
            }

            return View(task);
        }

        [HttpPost()]
        public async Task<IActionResult> GetAllRunningTaskList(int? goalId)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            // Get all tasks from the service
            //var data = await _taskService.GetAllTasks(userId, Status.Running);
            
            IEnumerable<TaskDTO> data = new List<TaskDTO>();

            if (goalId != null && goalId.HasValue && goalId.Value > 0)
            {
                data = await _taskService.GetAllTasksWithStatusByGoalId(userId, goalId.Value, Status.Running);
            }
            else
            {
                data = await _taskService.GetAllTasks(userId, Status.Running);
            }

            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.Name.ToLower().Contains(searchValue) || x.Id.ToString().Contains(searchValue));
            }

            // Get filtered record count after search
            filterRecord = data.Count();

            // Apply sorting if there is a valid column and direction
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                switch (sortColumn)
                {
                    case "Name":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Name) : data.OrderByDescending(x => x.Name);
                        break;
                    case "EndDate":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                        break;
                    case "TaskStatus":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskStatus) : data.OrderByDescending(x => x.TaskStatus);
                        break;
                    case "Priority":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Priority) : data.OrderByDescending(x => x.Priority);
                        break;
                    case "Repeat":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Repeat) : data.OrderByDescending(x => x.Repeat);
                        break;
                    case "Id":
                        // Sorting by Id (numerical)
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
                        break;
                    default:
                        data = data.OrderBy(x => x.Id); // Default sort by Id if no valid column is provided
                        break;
                }
            }

            // Paginate the data (skip and take)
            var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();

            // Map the data to TaskDTO using AutoMapper
            var returnObj = new
            {
                draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = _mapper.Map<List<TaskDTO>>(datas)
            };

            // Return the result as JSON
            return Json(returnObj);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllCompletedTaskList(int? goalId)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            // Get all tasks from the service
            //var data = await _taskService.GetAllTasks(userId, Status.Completed);
            IEnumerable<TaskDTO> data = new List<TaskDTO>();

            if (goalId != null && goalId.HasValue && goalId.Value > 0)
            {
                data = await _taskService.GetAllTasksWithStatusByGoalId(userId, goalId.Value, Status.Completed);
            }
            else
            {
                data = await _taskService.GetAllTasks(userId, Status.Completed);
            }

            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.Name!.ToLower().Contains(searchValue) || x.Id.ToString().Contains(searchValue));
            }

            // Get filtered record count after search
            filterRecord = data.Count();

            // Apply sorting if there is a valid column and direction
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                switch (sortColumn)
                {
                    case "Name":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Name) : data.OrderByDescending(x => x.Name);
                        break;
                    case "EndDate":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                        break;
                    case "TaskStatus":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskStatus) : data.OrderByDescending(x => x.TaskStatus);
                        break;
                    case "Priority":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Priority) : data.OrderByDescending(x => x.Priority);
                        break;
                    case "Repeat":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Repeat) : data.OrderByDescending(x => x.Repeat);
                        break;
                    case "Id":
                        // Sorting by Id (numerical)
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
                        break;
                    default:
                        data = data.OrderBy(x => x.Id); // Default sort by Id if no valid column is provided
                        break;
                }
            }

            // Paginate the data (skip and take)
            var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();

            // Map the data to TaskDTO using AutoMapper
            var returnObj = new
            {
                draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = _mapper.Map<List<TaskDTO>>(datas)
            };

            // Return the result as JSON
            return Json(returnObj);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllNotStartedTaskList(int? goalId)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            // Get all tasks from the service
            //var data = await _taskService.GetAllTasks(userId, Status.NotStarted);

            IEnumerable<TaskDTO> data = new List<TaskDTO>();

            if (goalId != null && goalId.HasValue && goalId.Value > 0)
            {
                data = await _taskService.GetAllTasksWithStatusByGoalId(userId, goalId.Value, Status.NotStarted);
            }
            else
            {
                data = await _taskService.GetAllTasks(userId, Status.NotStarted);
            }

            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.Name!.ToLower().Contains(searchValue) || x.Id.ToString().Contains(searchValue));
            }

            // Get filtered record count after search
            filterRecord = data.Count();

            // Apply sorting if there is a valid column and direction
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                switch (sortColumn)
                {
                    case "Name":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Name) : data.OrderByDescending(x => x.Name);
                        break;
                    case "EndDate":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                        break;
                    case "TaskStatus":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskStatus) : data.OrderByDescending(x => x.TaskStatus);
                        break;
                    case "Priority":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Priority) : data.OrderByDescending(x => x.Priority);
                        break;
                    case "Repeat":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Repeat) : data.OrderByDescending(x => x.Repeat);
                        break;
                    case "Id":
                        // Sorting by Id (numerical)
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
                        break;
                    default:
                        data = data.OrderBy(x => x.Id); // Default sort by Id if no valid column is provided
                        break;
                }
            }

            // Paginate the data (skip and take)
            var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();

            // Map the data to TaskDTO using AutoMapper
            var returnObj = new
            {
                draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = _mapper.Map<List<TaskDTO>>(datas)
            };

            // Return the result as JSON
            return Json(returnObj);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllEndedTaskList(int? goalId)
        {

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int totalRecord = 0;
            int filterRecord = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            // Get all tasks from the service
            //var data = await _taskService.GetAllTasks(userId, Status.Ended);

            IEnumerable<TaskDTO> data = new List<TaskDTO>();

            if (goalId != null && goalId.HasValue && goalId.Value > 0)
            {
                data = await _taskService.GetAllTasksWithStatusByGoalId(userId, goalId.Value, Status.Ended);
            }
            else
            {
                data = await _taskService.GetAllTasks(userId, Status.Ended);
            }

            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.Name!.ToLower().Contains(searchValue) || x.Id.ToString().Contains(searchValue));
            }

            // Get filtered record count after search
            filterRecord = data.Count();

            // Apply sorting if there is a valid column and direction
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                switch (sortColumn)
                {
                    case "Name":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Name) : data.OrderByDescending(x => x.Name);
                        break;
                    case "EndDate":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                        break;
                    case "TaskStatus":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskStatus) : data.OrderByDescending(x => x.TaskStatus);
                        break;
                    case "Priority":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Priority) : data.OrderByDescending(x => x.Priority);
                        break;
                    case "Repeat":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Repeat) : data.OrderByDescending(x => x.Repeat);
                        break;
                    case "Id":
                        // Sorting by Id (numerical)
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
                        break;
                    default:
                        data = data.OrderBy(x => x.Id); // Default sort by Id if no valid column is provided
                        break;
                }
            }

            // Paginate the data (skip and take)
            var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();

            // Map the data to TaskDTO using AutoMapper
            var returnObj = new
            {
                draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = _mapper.Map<List<TaskDTO>>(datas)
            };

            // Return the result as JSON
            return Json(returnObj);
        }

        //[HttpPost]
        //public async Task<IActionResult> GetAllDeletedTaskList()
        //{
        //    var userId = _userManager.GetUserId(User);

        //    if (userId == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    int totalRecord = 0;
        //    int filterRecord = 0;
        //    var draw = Request.Form["draw"].FirstOrDefault();
        //    var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
        //    var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
        //    var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
        //    var searchValue = Request.Form["search[value]"].FirstOrDefault();
        //    int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
        //    int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

        //    // Get all tasks from the service
        //    var data = await _taskService.GetAllTasks(userId, Status.Deleted);

        //    // Get total count of records
        //    totalRecord = data.Count();

        //    // Apply search filter if there's a search value
        //    if (!string.IsNullOrEmpty(searchValue))
        //    {
        //        // Perform case-insensitive search on multiple fields (Name and Id)
        //        data = data.Where(x => x.Name?.ToLower() == searchValue.ToLower() || x.Id.ToString().Contains(searchValue));
        //    }

        //    // Get filtered record count after search
        //    filterRecord = data.Count();

        //    // Apply sorting if there is a valid column and direction
        //    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
        //    {
        //        switch (sortColumn)
        //        {
        //            case "Name":
        //                data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Name) : data.OrderByDescending(x => x.Name);
        //                break;
        //            case "EndDate":
        //                data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
        //                break;
        //            case "TaskStatus":
        //                data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskStatus) : data.OrderByDescending(x => x.TaskStatus);
        //                break;
        //            case "Id":
        //                // Sorting by Id (numerical)
        //                data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
        //                break;
        //            default:
        //                data = data.OrderBy(x => x.Id); // Default sort by Id if no valid column is provided
        //                break;
        //        }
        //    }

        //    // Paginate the data (skip and take)
        //    var empList = data.Skip(skip).Take(pageSize).ToList();

        //    // Map the data to TaskDTO using AutoMapper
        //    var returnObj = new
        //    {
        //        draw = draw,
        //        recordsTotal = totalRecord,
        //        recordsFiltered = filterRecord,
        //        data = _mapper.Map<List<TaskDTO>>(empList)
        //    };

        //    // Return the result as JSON
        //    return Json(returnObj);
        //}

        [HttpDelete]
        public async Task<IActionResult> DeleteTask(int Id)
        {
            _logger.LogInformation("Entered DeleteTask with Id={Id}", Id);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in DeleteTask.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _taskService.DeleteTask(userId, Id);
                _logger.LogInformation("Task deleted for Id={Id}, userId={UserId}", Id, userId);
                return Json(new { status = true, message = $"Task with Id: {Id} deleted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in DeleteTask for Id={Id}, userId={UserId}", Id, userId);
                return Json(new { status = false, message = "Error occurred while deleting task." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeTaskStatus(int Id, [Bind("Id,TaskStatus")] TaskStatusDTO taskUpdate)
        {
            _logger.LogInformation("Entered ChangeTaskStatus with Id={Id}, TaskStatus={TaskStatus}", Id, taskUpdate.TaskStatus);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in ChangeTaskStatus.");
                return RedirectToAction("Login", "Account");
            }

            if (Id != taskUpdate.Id)
            {
                _logger.LogWarning("Invalid Parameter Id in ChangeTaskStatus. Expected: {ExpectedId}, Received: {ActualId}", taskUpdate.Id, Id);
                return Json(new { status = false, message = "Invalid Parameter Id!" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _taskService.UpdateTaskStatus(userId, taskUpdate.Id, taskUpdate.TaskStatus);
                    _logger.LogInformation("Task status updated for Id={Id}, userId={UserId}, newStatus={TaskStatus}", Id, userId, taskUpdate.TaskStatus);
                    return Json(new { status = true, message = $"Task with Id {Id} Status changed to {taskUpdate.TaskStatus}!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception in ChangeTaskStatus for Id={Id}, userId={UserId}", Id, userId);
                    return Json(new { status = false, message = "Error occurred while changing task status." });
                }
            }

            _logger.LogWarning("ModelState invalid in ChangeTaskStatus for Id={Id}.", Id);
            return Json(new { status = false, message = "ModelState is not valid!" });
        }

        [HttpGet]
        public async Task<IActionResult> SearchTasks(string query)
        {
            _logger.LogInformation("Entered SearchTasks with query={Query}", query);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in SearchTasks.");
                return Unauthorized();
            }

            try
            {
                var tasks = await _taskService.SearchTasks(userId, query);
                return Json(new { status = true, data = tasks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SearchTasks for query={Query}, userId={UserId}", query, userId);
                return Json(new { status = false, message = "Error occurred while searching tasks." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchTasksWithGoals(string query, Status status = Status.Running)
        {
            _logger.LogInformation("Entered SearchTasksWithGoals with query={Query}, status={Status}", query, status);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in SearchTasksWithGoals.");
                return Unauthorized();
            }

            try
            {
                var tasks = await _taskService.SearchTasksWithGoals(userId, query, status);
                return Json(new { status = true, data = tasks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SearchTasksWithGoals for query={Query}, userId={UserId}", query, userId);
                return Json(new { status = false, message = "Error occurred while searching tasks." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotesByTask(int taskId, int pageNumber = 1, int pageSize = 5)
        {
            _logger.LogInformation("Entered GetNotesByTask with taskId={TaskId}, pageNumber={PageNumber}, pageSize={PageSize}", taskId, pageNumber, pageSize);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetNotesByTask.");
                return Unauthorized();
            }

            try
            {
                var notes = await _notesService.GetAllNotesByTaskId(userId, taskId, Status.All, pageNumber, pageSize);
                return Json(new { status = true, message = "Task notes loaded", data = notes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetNotesByTask for taskId={TaskId}, userId={UserId}", taskId, userId);
                return Json(new { status = false, message = "Error loading notes" });
            }
        }
    }
}
