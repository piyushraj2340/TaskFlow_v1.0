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

            TodoProgressAnalysisDTO productivity = await _service.GetTodoProgressAnalyses(userId, DateTime.Now.Date);
            return View(productivity);
        }

        [HttpPost]
        public async Task<IActionResult> GetTodoProgressAnalyses(DateTime forDate, int? taskId)
        {
            var userId = _userManager.GetUserId(User);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if(taskId != null && taskId.HasValue)
                {
                    var productivity = await _service.GetTodoProgressAnalyses(userId, taskId.Value);
                    return Json(new { status = true, message = "Todo Task Progress", data = productivity });
                } else
                {
                    var productivity = await _service.GetTodoProgressAnalyses(userId, forDate);
                    return Json(new { status = true, message = "Todo Task Progress", data = productivity });
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetRunningTodo(DateTime? selectDate, int? taskId)
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

            // Get all goals from the service
            IEnumerable<TodoDTOWithTaskDTO> data;

            if (taskId != null && taskId.HasValue)
            {
                if (selectDate.HasValue)
                {
                    data = await _service.GetAllTodo(userId, taskId.Value, Status.Running, selectDate.Value);
                }
                else
                {
                    data = await _service.GetAllTodo(userId, taskId.Value, Status.Running);

                }
            }
            else
            {
                if (selectDate.HasValue)
                {
                    data = await _service.GetAllTodo(userId, Status.Running, selectDate.Value);
                }
                else
                {
                    data = await _service.GetAllTodo(userId, Status.Running);

                }
            }


            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.TaskName.ToLower().Contains(searchValue) ||
                        x.Id.ToString().Contains(searchValue)
                        );
            }

            // Get filtered record count after search
            filterRecord = data.Count();

            // Apply sorting if there is a valid column and direction
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                switch (sortColumn)
                {
                    case "name":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskName) : data.OrderByDescending(x => x.TaskName);
                        break;
                    case "endDate":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                        break;
                    case "priority":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskPriority) : data.OrderByDescending(x => x.TaskPriority);
                        break;
                    case "repeat":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskPriority) : data.OrderByDescending(x => x.TaskPriority);
                        break;
                    case "id":
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

            var todoData = _mapper.Map<List<TodoDTO>>(datas);

            //if(todoData.) 

            // Map the data to GoalDTO using AutoMapper
            var returnObj = new
            {
                draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = todoData
            };

            // Return the result as JSON
            return Json(returnObj);
        }


        [HttpPost]
        public async Task<IActionResult> GetCompletedTodo(DateTime? selectDate, int? taskId)
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

            // Get all goals from the service
            IEnumerable<TodoDTOWithTaskDTO> data;
            if (taskId != null && taskId.HasValue)
            {
                if (selectDate.HasValue)
                {
                    data = await _service.GetAllTodo(userId, taskId.Value, Status.Completed, selectDate.Value);
                }
                else
                {
                    data = await _service.GetAllTodo(userId, taskId.Value, Status.Completed);

                }
            }
            else
            {
                if (selectDate.HasValue)
                {
                    data = await _service.GetAllTodo(userId, Status.Completed, selectDate.Value);
                }
                else
                {
                    data = await _service.GetAllTodo(userId, Status.Completed);

                }
            }

            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.TaskName.ToLower().Contains(searchValue) ||
                        x.Id.ToString().Contains(searchValue)
                        );
            }

            // Get filtered record count after search
            filterRecord = data.Count();

            // Apply sorting if there is a valid column and direction
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                switch (sortColumn)
                {
                    case "name":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskName) : data.OrderByDescending(x => x.TaskName);
                        break;
                    case "endDate":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                        break;
                    case "priority":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskPriority) : data.OrderByDescending(x => x.TaskPriority);
                        break;
                    case "repeat":
                        data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskPriority) : data.OrderByDescending(x => x.TaskPriority);
                        break;
                    case "id":
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
            var todoData = _mapper.Map<List<TodoDTO>>(datas);

            //if(todoData.) 

            // Map the data to GoalDTO using AutoMapper
            var returnObj = new
            {
                draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = todoData
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

                await _service.UpdateTodoStatus(userId, todoUpdate.Id, todoUpdate.Status);
                return Json(new { status = true, message = $"Todo with Id {Id} Status change to Completed!" });
            }

            return Json(new { status = false, message = "ModelState is not valid!" });
        }

        [HttpPost]
        public async Task<IActionResult> SaveTodoNotes(int todoId, string notes)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            await _service.UpdateTodoNotes(userId, todoId, notes);
            return Json(new { status = true, message = "Notes saved successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetTodoById(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var todo = await _service.GetTodoById(userId, id);
                return Json(new { status = true, message = "Todo fetched", data = todo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching todo with id {TodoId} for user {UserId}", id, userId);
                return Json(new { status = false, message = ex.Message });
            }
        }
    }
}
