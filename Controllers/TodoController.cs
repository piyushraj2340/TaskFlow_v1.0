using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
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
            _logger.LogInformation("Entered Index action.");
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Index.");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("Fetching todo progress for user {UserId}.", userId);
            TodoProgressAnalysisDTO productivity = await _service.GetTodoProgressAnalysesWithoutSpAsync(userId, DateTime.Now.Date);
            _logger.LogInformation("Fetched productivity for user {UserId}: {@Productivity}", userId, productivity);
            return View(productivity);
        }

        [HttpPost]
        public async Task<IActionResult> GetTodoProgressAnalyses(DateTime forDate, int? taskId)
        {
            _logger.LogInformation("Entered GetTodoProgressAnalyses with forDate={ForDate}, taskId={TaskId}", forDate, taskId);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetTodoProgressAnalyses.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (taskId != null && taskId.HasValue)
                {
                    _logger.LogInformation("Fetching productivity for user {UserId} and taskId {TaskId}.", userId, taskId.Value);
                    var productivity = await _service.GetTodoProgressAnalyses(userId, taskId.Value);
                    return Json(new { status = true, message = "Todo Task Progress", data = productivity });
                }
                else
                {
                    _logger.LogInformation("Fetching productivity for user {UserId} and date {ForDate}.", userId, forDate);
                    var productivity = await _service.GetTodoProgressAnalysesWithoutSpAsync(userId, forDate);
                    return Json(new { status = true, message = "Todo Task Progress", data = productivity });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetTodoProgressAnalyses for user {UserId}, forDate={ForDate}, taskId={TaskId}", userId, forDate, taskId);
                return Json(new { status = false, message = "Error occurred while fetching productivity." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetRunningTodo(DateTime? selectDate, int? taskId)
        {
            _logger.LogInformation("Entered GetRunningTodo with selectDate={SelectDate}, taskId={TaskId}", selectDate, taskId);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetRunningTodo.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Log DataTable parameters
                var draw = Request.Form["draw"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
                int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
                int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

                _logger.LogInformation("DataTable params: draw={Draw}, sortColumn={SortColumn}, sortDirection={SortDirection}, searchValue={SearchValue}, pageSize={PageSize}, skip={Skip}",
                    draw, sortColumn, sortColumnDirection, searchValue, pageSize, skip);

                IEnumerable<TodoDTOWithTaskDTO> todoDataWithTask;

                if (taskId != null && taskId.HasValue)
                {
                    if (selectDate.HasValue)
                    {
                        todoDataWithTask = await _service.GetAllTodo(userId, taskId.Value, Status.Running, selectDate.Value);
                    }
                    else
                    {
                        todoDataWithTask = await _service.GetAllTodo(userId, taskId.Value, Status.Running);
                    }
                }
                else
                {
                    if (selectDate.HasValue)
                    {
                        todoDataWithTask = await _service.GetAllTodo(userId, Status.Running, selectDate.Value);
                    }
                    else
                    {
                        todoDataWithTask = await _service.AddAndGetTodosFromTaskAsync(userId, Status.Running);
                    }
                }

                var data = _mapper.Map<IEnumerable<TodoWithTaskProductivityDTO>>(todoDataWithTask);

                foreach (var d in data)
                {
                    if (d != null && d.TaskId > 0)
                    {
                        var productivity = await _service.GetTodoProgressAnalyses(userId, d.TaskId);
                        d.TaskProductivity = productivity;
                    }
                }

                int totalRecord = data.Count();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(x =>
                        (x.TaskName?.ToLower().Contains(searchValue) ?? false) ||
                        x.Id.ToString().Contains(searchValue) ||
                        (x.Notes?.ToLower().Contains(searchValue) ?? false)
                    );
                }

                int filterRecord = data.Count();

                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    _logger.LogInformation("Sorting by {SortColumn} {SortDirection}", sortColumn, sortColumnDirection);
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
                        case "productivity":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskProductivity?.ProductivityForDay) : data.OrderByDescending(x => x.TaskProductivity?.ProductivityForDay);
                            break;
                        case "repeat":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskPriority) : data.OrderByDescending(x => x.TaskPriority);
                            break;
                        case "id":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
                            break;
                        default:
                            data = data.OrderBy(x => x.Id);
                            break;
                    }
                }

                var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();
                var todoData = _mapper.Map<List<TodoDTO>>(datas);

                var returnObj = new
                {
                    draw,
                    recordsTotal = totalRecord,
                    recordsFiltered = filterRecord,
                    data = todoData
                };

                _logger.LogInformation("Returning {Count} running todos (filtered: {FilteredCount})", todoData.Count, filterRecord);
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetRunningTodo for user {UserId}", userId);
                return Json(new { status = false, message = "Error occurred while fetching running todos." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetCompletedTodo(DateTime? selectDate, int? taskId)
        {
            _logger.LogInformation("Entered GetCompletedTodo with selectDate={SelectDate}, taskId={TaskId}", selectDate, taskId);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetCompletedTodo.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
                int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
                int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

                IEnumerable<TodoDTOWithTaskDTO> todoDataWithTask;

                if (taskId != null && taskId.HasValue)
                {
                    if (selectDate.HasValue)
                    {
                        todoDataWithTask = await _service.GetAllTodo(userId, taskId.Value, Status.Completed, selectDate.Value);
                    }
                    else
                    {
                        todoDataWithTask = await _service.GetAllTodo(userId, taskId.Value, Status.Completed);
                    }
                }
                else
                {
                    if (selectDate.HasValue)
                    {
                        todoDataWithTask = await _service.GetAllTodo(userId, Status.Completed, selectDate.Value);
                    }
                    else
                    {
                        todoDataWithTask = await _service.GetAllTodo(userId, Status.Completed);
                    }
                }

                var data = _mapper.Map<IEnumerable<TodoWithTaskProductivityDTO>>(todoDataWithTask);

                foreach (var d in data)
                {
                    if (d != null && d.TaskId > 0)
                    {
                        var productivity = await _service.GetTodoProgressAnalyses(userId, d.TaskId);
                        d.TaskProductivity = productivity;
                    }
                }

                int totalRecord = data.Count();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(x =>
                        (x.TaskName?.ToLower().Contains(searchValue) ?? false) ||
                        x.Id.ToString().Contains(searchValue) ||
                        (x.Notes?.ToLower().Contains(searchValue) ?? false)
                    );
                }

                int filterRecord = data.Count();

                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    _logger.LogInformation("Sorting by {SortColumn} {SortDirection}", sortColumn, sortColumnDirection);
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
                        case "productivity":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskProductivity?.ProductivityForDay) : data.OrderByDescending(x => x.TaskProductivity?.ProductivityForDay);
                            break;
                        case "repeat":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.TaskPriority) : data.OrderByDescending(x => x.TaskPriority);
                            break;
                        case "id":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
                            break;
                        default:
                            data = data.OrderBy(x => x.Id);
                            break;
                    }
                }

                var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();
                var todoData = _mapper.Map<List<TodoDTO>>(datas);

                var returnObj = new
                {
                    draw,
                    recordsTotal = totalRecord,
                    recordsFiltered = filterRecord,
                    data = todoData
                };

                _logger.LogInformation("Returning {Count} completed todos (filtered: {FilteredCount})", todoData.Count, filterRecord);
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetCompletedTodo for user {UserId}", userId);
                return Json(new { status = false, message = "Error occurred while fetching completed todos." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeTodoStatus(int Id, [Bind("Id,Status")] TodoStatusDTO todoUpdate)
        {
            _logger.LogInformation("Entered ChangeTodoStatus with Id={Id}, Status={Status}", Id, todoUpdate.Status);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in ChangeTodoStatus.");
                return RedirectToAction("Login", "Account");
            }

            if (Id != todoUpdate.Id)
            {
                _logger.LogWarning("Invalid Parameter Id in ChangeTodoStatus. Expected: {ExpectedId}, Received: {ActualId}", todoUpdate.Id, Id);
                return Json(new { status = false, message = "Invalid Parameter Id!" });
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Updating todo status for user {UserId}, todoId={TodoId}, status={Status}", userId, todoUpdate.Id, todoUpdate.Status);
                await _service.UpdateTodoStatus(userId, todoUpdate.Id, todoUpdate.Status);
                return Json(new { status = true, message = $"Todo with Id {Id} Status change to Completed!" });
            }

            _logger.LogWarning("Invalid ModelState in ChangeTodoStatus for todoId={TodoId}", Id);
            return Json(new { status = false, message = "ModelState is not valid!" });
        }

        [HttpPost]
        public async Task<IActionResult> SaveTodoNotes(int todoId, string notes)
        {
            _logger.LogInformation("Entered SaveTodoNotes with todoId={TodoId}", todoId);
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in SaveTodoNotes.");
                return Unauthorized();
            }

            try
            {
                await _service.UpdateTodoNotes(userId, todoId, notes);
                _logger.LogInformation("Notes saved for todoId={TodoId} by user {UserId}", todoId, userId);
                return Json(new { status = true, message = "Notes saved successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SaveTodoNotes for todoId={TodoId}, userId={UserId}", todoId, userId);
                return Json(new { status = false, message = "Error occurred while saving notes." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTodoById(int id)
        {
            _logger.LogInformation("Entered GetTodoById with id={Id}", id);
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetTodoById.");
                return Unauthorized();
            }

            try
            {
                var todo = await _service.GetTodoById(userId, id);
                _logger.LogInformation("Fetched todo for id={Id}, userId={UserId}", id, userId);
                return Json(new { status = true, message = "Todo fetched", data = todo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching todo with id {TodoId} for user {UserId}", id, userId);
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddBulkTodos([FromBody] BulkTodoCreateDTO bulkDto)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            try
            {
                await _service.AddBulkTodos(userId, bulkDto);
                return Json(new { status = true, message = "Todos created successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk todo creation failed for user {UserId}", userId);
                return Json(new { status = false, message = ex.Message });
            }
        }
    }
}
