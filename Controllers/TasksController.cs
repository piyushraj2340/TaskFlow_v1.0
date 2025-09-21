using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using TaskMonitoringApp.Models.Business;
using TaskMonitoringApp.Models.DataAccessLayer;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;
using TaskMonitoringApp.Models.ViewModel;

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

        public TasksController(ILogger<TasksController> logger, IMapper mapper, ITaskServices taskService, INotesServices notesService, IGoalServices goalsService, UserManager<Users> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _taskService = taskService;
            _notesService = notesService;
            _goalsService = goalsService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }


            TaskProductivityDTO productivity = await _taskService.GetTaskProductivity(userId);
            return View(productivity);
        }

        [Route("{controller}/{action}/{Id}/{tabName?}")]
        public async Task<IActionResult> Details(int Id, string? tabName)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var task = await _taskService.GetAllGoalNamesWithStatusAndTask(userId, Id, Status.All);

            if (task == null)
            {
                return NotFound();
            }


            ViewBag.tabName = tabName;

            var notesList = await _notesService.GetAllNotesByTaskId(userId, Id, Status.All);
            var taskWithNoteList = _mapper.Map<TaskViewModel>(task);

            taskWithNoteList.NotesLists = notesList;
            taskWithNoteList.GoalLists = task.GoalLists;




            return View(taskWithNoteList);



            //var task = await _taskService.GetAllGoalNamesWithStatusAndTask(userId, Id, Status.All);
            //TaskViewModel taskDetail = _mapper.Map<TaskViewModel>(task);
            //taskDetail.GoalLists = task.GoalLists;

            //return View(taskDetail);
        }

        public async Task<IActionResult> Create(int? goalId)
        {
            ViewBag.IsEditMode = false;
            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString(); // store the previous page that come from...

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
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

                    return View(emptyTask);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,Description,Repeat,RepeatWeekList,Priority,EndDate,TasksList,GoalIds,StartOptionType,StartDate")] TaskViewModel task)
        {
                var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }



            try
            {
                ViewBag.IsEditMode = false;
                var returnUrl = string.IsNullOrWhiteSpace(TempData["ReturnUrl"]?.ToString()) ? Url.Action("Index", "Home") : TempData["ReturnUrl"]?.ToString();

                //Validations
                if (task.StartOptionType == StartOptions.Scheduled && task.StartDate == null)
                {
                    ModelState.AddModelError("StartDate", "Start Date Must be required for scheduled!");
                }

                if (task.StartDate >= task?.EndDate?.Date)
                {
                    ModelState.AddModelError("StartDate", "Oops! The end date cannot be before or the same as the start date. Please select a later date.");
                }

                if (DateTime.Now > task?.EndDate)
                {
                    ModelState.AddModelError("EndDate", "The end date must be in the future.");
                }


                if (ModelState.IsValid)
                {

                    _logger.LogInformation("Attempting to Add new Task: {TaskName}", task?.Name);


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

                    // Log success after adding the product
                    _logger.LogInformation("Task '{TaskName}' successfully added with ID: {TaskID}", task.Name, task.Id);

                    return Redirect(returnUrl ?? "/");

                }
            }
            catch (ArgumentException ae)
            {
                ModelState.AddModelError("", !string.IsNullOrWhiteSpace(ae.Message) ? ae.Message : "Invalid Input Fields");
            }
            catch (Exception)
            {
                throw;
            }

            return View(task);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.IsEditMode = true;
            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString(); // store the previous page that come from...

            var taskToEdit = await _taskService.GetAllGoalNamesWithStatusAndTask(userId, id, Status.All);
            if (taskToEdit == null)
            {
                return NotFound();
            }
            TaskViewModel taskViewModel = _mapper.Map<TaskViewModel>(taskToEdit);
            taskViewModel.GoalLists = taskToEdit.GoalLists;

            if (taskViewModel != null && taskViewModel.GoalLists != null)
            {
                taskViewModel.GoalIds = string.Join(",", taskViewModel.GoalLists.Select(gId => gId.Id));
            }

            return View(taskViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Repeat,TaskStatus,RepeatWeekList,Priority,EndDate,TasksList,GoalIds,StartOptionType,StartDate")] TaskViewModel task)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != task.Id)
            {
                return NotFound();
            }
            task.UserId = userId;

            ViewBag.IsEditMode = true;
            var returnUrl = string.IsNullOrWhiteSpace(TempData["ReturnUrl"]?.ToString()) ? Url.Action("Index", "Home") : TempData["ReturnUrl"]?.ToString();


            //Validations
            if (task.StartOptionType == StartOptions.Scheduled && task.StartDate == null)
            {
                ModelState.AddModelError("StartDate", "Start Date Must be required for scheduled!");
            }

            if (task.StartDate >= task?.EndDate?.Date)
            {
                ModelState.AddModelError("StartDate", "Oops! The end date cannot be before or the same as the start date. Please select a later date.");
            }

            if (DateTime.Now > task?.EndDate)
            {
                ModelState.AddModelError("EndDate", "The end date must be in the future.");
            }


            if (ModelState.IsValid) //Todo: check for this conditions 
            {

                _logger.LogInformation("Attempting to Update new Task: {TaskName}", task.Name);

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

                // Log success after adding the product
                _logger.LogInformation("Task '{TaskName}' successfully Updated with ID: {TaskID}", task.Name, task.Id);

                return Redirect(returnUrl ?? "/");
            }
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllRunningTaskList()
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
            var data = await _taskService.GetAllTasks(userId, Status.Running);

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
        public async Task<IActionResult> GetAllCompletedTaskList()
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
            var data = await _taskService.GetAllTasks(userId, Status.Completed);

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
        public async Task<IActionResult> GetAllNotStartedTaskList()
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
            var data = await _taskService.GetAllTasks(userId, Status.NotStarted);

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
        public async Task<IActionResult> GetAllEndedTaskList()
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
            var data = await _taskService.GetAllTasks(userId, Status.Ended);

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
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await _taskService.DeleteTask(userId, Id);
            return Json(new { status = true, message = $"Task with Id: ${Id} deleted successfully!." });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeTaskStatus(int Id, [Bind("Id,TaskStatus")] TaskStatusDTO taskUpdate)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Id != taskUpdate.Id) return Json(new { status = false, message = "Invalid Parameter Id!" });

            if (ModelState.IsValid)
            {
                await _taskService.UpdateTaskStatus(userId, taskUpdate.Id, taskUpdate.TaskStatus);
                return Json(new { status = true, message = $"Task with Id {Id} Status change to Completed!" });
            }

            return Json(new { status = false, message = "ModelState is not valid!" });
        }
    }
}
