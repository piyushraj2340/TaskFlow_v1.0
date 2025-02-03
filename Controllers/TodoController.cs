using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ILogger<TodoController> _logger;
        private readonly IMapper _mapper;
        private readonly ITodoServices _service;
        private readonly UserManager<Users> _userManager;

        public TodoController(ILogger<TodoController> logger, IMapper mapper, ITodoServices service, UserManager<Users> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _service = service;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            TodoProductivityDTO productivity = await _service.GetProgressForTodays(userId);
            return View(productivity);
        }

        [HttpPost]
        public async Task<IActionResult> GetTodoProgressForTodays()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                TodoProductivityDTO productivity = await _service.GetProgressForTodays(userId);
                return Json(new { status = true, message = "Todo Task Progress", data = productivity });
            } catch(Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetRunningTodo()
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
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            // Get all goals from the service
            var data = await _service.GetAllTodo(userId, Status.Running);

            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.Task?.Name?.ToLower() == searchValue.ToLower() || x.Id.ToString().Contains(searchValue));
            }

            // Get filtered record count after search
            filterRecord = data.Count();

            // Apply sorting if there is a valid column and direction
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                switch (sortColumn)
                {
                    case "Name":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Task?.Name) : data.OrderByDescending(x => x.Task?.Name);
                        break;
                    case "EndDate":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                        break;
                    case "Priority":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Task?.Priority) : data.OrderByDescending(x => x.Task?.Priority);
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
            var empList = data.Skip(skip).Take(pageSize).ToList();

            // Map the data to GoalDTO using AutoMapper
            var returnObj = new
            {
                draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = _mapper.Map<List<TodoDTO>>(empList)
            };

            // Return the result as JSON
            return Json(returnObj);
        }


        [HttpPost]
        public async Task<IActionResult> GetCompletedTodo()
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
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            // Get all goals from the service
            var data = await _service.GetAllTodo(userId, Status.Completed);

            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.Task?.Name?.ToLower() == searchValue.ToLower() || x.Id.ToString().Contains(searchValue));
            }

            // Get filtered record count after search
            filterRecord = data.Count();

            // Apply sorting if there is a valid column and direction
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                switch (sortColumn)
                {
                    case "Name":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Task?.Name) : data.OrderByDescending(x => x.Task?.Name);
                        break;
                    case "EndDate":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                        break;
                    case "Priority":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Task?.Priority) : data.OrderByDescending(x => x.Task?.Priority);
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
            var empList = data.Skip(skip).Take(pageSize).ToList();

            // Map the data to GoalDTO using AutoMapper
            var returnObj = new
            {
                draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = _mapper.Map<List<TodoDTO>>(empList)
            };

            // Return the result as JSON
            return Json(returnObj);
        }


        [HttpPost]
        public async Task<IActionResult> ChangeTodoStatus(int Id, [Bind("Id,Status")] TodoStatusDTO todoUpdate)
        {

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }


            if (Id != todoUpdate.Id) return Json(new { status = false, message = "Invalid Parameter Id!" });

            if (ModelState.IsValid)
            {

                var taskInfo = await _service.GetTodoById(userId, Id);
                taskInfo = _mapper.Map(todoUpdate, taskInfo);

                var user = await _userManager.FindByIdAsync(userId);
                
                if(user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                taskInfo.User = user;

                // handling to mark as complete...
                if (todoUpdate.Status == Status.Completed)
                {
                    await _service.MarkAsComplete(userId, taskInfo);

                    return Json(new { status = true, message = $"Task with Id {Id} Status change to Completed!" });
                }
                else if (todoUpdate.Status == Status.Running) // handling to move to running
                {
                    await _service.MoveToRunning(userId, taskInfo);
                    return Json(new { status = true, message = $"Task with Id {Id} Status change to Running!" });
                }
                else if (todoUpdate.Status == Status.Ended)
                {
                    taskInfo.Status = Status.Ended; // changing the status to ended
                    taskInfo.EndDate = DateTime.Now; // making task to end early...

                    await _service.UpdateTodo(userId, taskInfo);
                    return Json(new { status = true, message = $"Task with Id {Id} Status change to End!" });
                }
                else if (todoUpdate.Status == Status.NotStarted)
                {
                    await _service.UpdateTodo(userId, taskInfo);
                    return Json(new { status = true, message = $"Task with Id {Id} Status change to NotStarted!" });
                }
                else
                {
                    return Json(new { status = false, message = "Invalid parameter status" });
                }
            }

            return Json(new { status = false, message = "ModelState is not valid!" });
        }
    }
}
