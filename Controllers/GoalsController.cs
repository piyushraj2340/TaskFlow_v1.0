using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;
using TaskMonitoringApp.Models.ViewModel;

namespace TaskMonitoringApp.Controllers
{
    [Authorize]
    public class GoalsController : Controller
    {
        private readonly ITaskServices _taskService;
        private readonly IGoalServices _goalService;
        private readonly INotesServices _noteService;
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<Users> _userManager;

        public GoalsController(ILogger<HomeController> logger, IGoalServices goalService, INotesServices noteService, IMapper mapper, UserManager<Users> userManager, ITaskServices taskService)
        {
            _goalService = goalService;
            _noteService = noteService;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _taskService = taskService;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Entered GoalsController.Index");
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Index.");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("Fetching goal productivity for user {UserId}.", userId);
            GoalProductivityDTO productivity = await _goalService.GetGoalProductivity(userId);
            _logger.LogInformation("Fetched productivity for user {UserId}: {@Productivity}", userId, productivity);

            return View(productivity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,EndDate,Priority,Description,StartOptionType,StartDate")] GoalViewModel goal)
        {
            _logger.LogInformation("Entered Create with Name={Name}, EndDate={EndDate}, Priority={Priority}, StartOptionType={StartOptionType}, StartDate={StartDate}", goal.Name, goal.EndDate, goal.Priority, goal.StartOptionType, goal.StartDate);

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Create.");
                return RedirectToAction("Login", "Account");
            }

            goal.UserId = userId;

            if (goal.StartOptionType == StartOptions.Scheduled && goal.StartDate == null)
            {
                ModelState.AddModelError("", "Start Date Must be required for scheduled!");
                _logger.LogWarning("Start date required for scheduled goal creation.");
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Attempting to add a new Goal: {GoalName}", goal.Name);

                if (goal.StartOptionType == StartOptions.Immediate)
                {
                    goal.IsScheduled = true;
                    goal.StartDate = DateTime.Now;
                }
                else if (goal.StartOptionType == StartOptions.Scheduled)
                {
                    goal.IsScheduled = true;
                }

                await _goalService.AddNewGoal(userId, _mapper.Map<GoalDTO>(goal));

                _logger.LogInformation("Goal '{GoalName}' successfully added with ID: {GoalID}", goal.Name, goal.Id);

                return Json(new { status = true, message = "New Goal Successfully Added", data = goal });
            }

            _logger.LogWarning("ModelState invalid in Create. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList());
            return Json(new { status = false, message = "ModelState is not valid!", ErrormessageList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
        }

        [Route("Goals/Details/{Id}/{tabName?}")]
        public async Task<IActionResult> Details(int Id, string? tabName)
        {
            _logger.LogInformation("Entered Details with Id={Id}, tabName={TabName}", Id, tabName);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Details.");
                return RedirectToAction("Login", "Account");
            }

            var goal = await _goalService.GetAllTaskNameWithStatusAndGoal(userId, Id, Status.All);

            if (goal == null)
            {
                _logger.LogWarning("Goal not found for Id={Id}, userId={UserId}", Id, userId);
                return NotFound();
            }

            TaskProductivityDTO productivity = await _taskService.GetTaskProductivity(userId, Id);

            ViewBag.tabName = tabName;

            var notesList = await _noteService.GetAllNotesByGoalId(userId, Id, Status.All, pageNumber: 1, pageSize: 20);
            var goalWithNoteList = _mapper.Map<GoalWithNotesAndTaskNameListViewModel>(goal);
            goalWithNoteList.NotesLists = notesList;
            goalWithNoteList.TaskProductivity = productivity;

            _logger.LogInformation("Returning details for goal Id={Id}, userId={UserId}", Id, userId);
            return View(goalWithNoteList);
        }

        public async Task<IActionResult> GetById(int Id)
        {
            _logger.LogInformation("Entered GetById with Id={Id}", Id);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetById.");
                return RedirectToAction("Login", "Account");
            }

            var goal = await _goalService.GetAllTaskNameWithStatusAndGoal(userId, Id, Status.All);

            if (goal == null)
            {
                _logger.LogWarning("Goal data not found for Id={Id}, userId={UserId}", Id, userId);
                return Json(new { status = false, message = "Goal Data Not Found!" });
            }

            _logger.LogInformation("Goal data found for Id={Id}, userId={UserId}", Id, userId);
            return Json(new { status = true, message = "Goal Data Found!", data = goal });
        }

        [HttpPut]
        public async Task<IActionResult> Edit([Bind("Id,Name,EndDate,GoalStatus,Description,Priority,StartOptionType,StartDate")] GoalViewModel goalData)
        {
            _logger.LogInformation("Entered Edit with Id={Id}, Name={Name}, EndDate={EndDate}, GoalStatus={GoalStatus}, Priority={Priority}, StartOptionType={StartOptionType}, StartDate={StartDate}", goalData.Id, goalData.Name, goalData.EndDate, goalData.GoalStatus, goalData.Priority, goalData.StartOptionType, goalData.StartDate);

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Edit.");
                return RedirectToAction("Login", "Account");
            }

            if (goalData.StartOptionType == StartOptions.Scheduled && goalData.StartDate == null)
            {
                ModelState.AddModelError("", "Start Date Must be required for scheduled!");
                _logger.LogWarning("Start date required for scheduled goal edit.");
            }

            if (ModelState.IsValid)
            {
                var goal = _mapper.Map<GoalDTO>(goalData);
                goal.UserId = userId;

                if (goal.StartOptionType == StartOptions.Immediate)
                {
                    goal.IsScheduled = true;
                    goal.StartDate = DateTime.Now;
                }
                else if (goal.StartOptionType == StartOptions.Scheduled)
                {
                    goal.IsScheduled = true;
                }
                else if (goal.StartOptionType == StartOptions.Manual)
                {
                    goal.IsScheduled = false;
                }

                await _goalService.UpdateGoal(userId, goal);

                _logger.LogInformation("Goal updated successfully for Id={Id}, userId={UserId}", goalData.Id, userId);
                return Json(new { status = true, message = "Goal updated successfully!" });
            }

            _logger.LogWarning("ModelState invalid in Edit for Id={Id}.", goalData.Id);
            return Json(new { status = false, message = "ModelState Invalid!" });
        }

        [HttpPost]
        public async Task<IActionResult> GetAllRunningGoals()
        {
            _logger.LogInformation("Entered GetAllRunningGoals");
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetAllRunningGoals.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                int totalRecord = 0;
                int filterRecord = 0;
                var draw = Request.Form["draw"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
                int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
                int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

                _logger.LogInformation("DataTable params: draw={Draw}, sortColumn={SortColumn}, sortDirection={SortDirection}, searchValue={SearchValue}, pageSize={PageSize}, skip={Skip}",
                    draw, sortColumn, sortColumnDirection, searchValue, pageSize, skip);

                var data = await _goalService.GetAllGoals(userId, Status.Running);

                totalRecord = data.Count();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(x => x.Name.ToLower().Contains(searchValue) ||
                            x.Id.ToString().Contains(searchValue) ||
                            x.Description.ToLower().Contains(searchValue)
                    );
                }

                filterRecord = data.Count();

                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    _logger.LogInformation("Sorting by {SortColumn} {SortDirection}", sortColumn, sortColumnDirection);
                    switch (sortColumn)
                    {
                        case "Name":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Name) : data.OrderByDescending(x => x.Name);
                            break;
                        case "Due Date":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                            break;
                        case "GoalStatus":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.GoalStatus) : data.OrderByDescending(x => x.GoalStatus);
                            break;
                        case "Priority":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Priority) : data.OrderByDescending(x => x.Priority);
                            break;
                        case "Status":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.GoalStatus) : data.OrderByDescending(x => x.GoalStatus);
                            break;
                        case "Id":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
                            break;
                        default:
                            data = data.OrderBy(x => x.Id);
                            break;
                    }
                }

                var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();

                var returnObj = new
                {
                    draw,
                    recordsTotal = totalRecord,
                    recordsFiltered = filterRecord,
                    data = _mapper.Map<List<GoalDTO>>(datas)
                };

                _logger.LogInformation("Returning {Count} running goals (filtered: {FilteredCount})", datas.Count, filterRecord);
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetAllRunningGoals for user {UserId}", userId);
                return Json(new { status = false, message = "Error occurred while fetching running goals." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAllCompletedGoals()
        {
            _logger.LogInformation("Entered GetAllCompletedGoals");
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetAllCompletedGoals.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                int totalRecord = 0;
                int filterRecord = 0;
                var draw = Request.Form["draw"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
                int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
                int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

                _logger.LogInformation("DataTable params: draw={Draw}, sortColumn={SortColumn}, sortDirection={SortDirection}, searchValue={SearchValue}, pageSize={PageSize}, skip={Skip}",
                    draw, sortColumn, sortColumnDirection, searchValue, pageSize, skip);

                var data = await _goalService.GetAllGoals(userId, Status.Completed);

                totalRecord = data.Count();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(x => x.Name.ToLower().Contains(searchValue) ||
                            x.Id.ToString().Contains(searchValue) ||
                            x.Description.ToLower().Contains(searchValue)
                    );
                }

                filterRecord = data.Count();

                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    _logger.LogInformation("Sorting by {SortColumn} {SortDirection}", sortColumn, sortColumnDirection);
                    switch (sortColumn)
                    {
                        case "Name":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Name) : data.OrderByDescending(x => x.Name);
                            break;
                        case "Due Date":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                            break;
                        case "GoalStatus":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.GoalStatus) : data.OrderByDescending(x => x.GoalStatus);
                            break;
                        case "Priority":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Priority) : data.OrderByDescending(x => x.Priority);
                            break;
                        case "Status":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.GoalStatus) : data.OrderByDescending(x => x.GoalStatus);
                            break;
                        case "Id":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
                            break;
                        default:
                            data = data.OrderBy(x => x.Id);
                            break;
                    }
                }

                var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();

                var returnObj = new
                {
                    draw,
                    recordsTotal = totalRecord,
                    recordsFiltered = filterRecord,
                    data = _mapper.Map<List<GoalDTO>>(datas)
                };

                _logger.LogInformation("Returning {Count} completed goals (filtered: {FilteredCount})", datas.Count, filterRecord);
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetAllCompletedGoals for user {UserId}", userId);
                return Json(new { status = false, message = "Error occurred while fetching completed goals." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAllNotStartedGoals()
        {
            _logger.LogInformation("Entered GetAllNotStartedGoals");
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetAllNotStartedGoals.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                int totalRecord = 0;
                int filterRecord = 0;
                var draw = Request.Form["draw"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
                int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
                int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

                _logger.LogInformation("DataTable params: draw={Draw}, sortColumn={SortColumn}, sortDirection={SortDirection}, searchValue={SearchValue}, pageSize={PageSize}, skip={Skip}",
                    draw, sortColumn, sortColumnDirection, searchValue, pageSize, skip);

                var data = await _goalService.GetAllGoals(userId, Status.NotStarted);

                totalRecord = data.Count();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(x => x.Name.ToLower().Contains(searchValue) ||
                            x.Id.ToString().Contains(searchValue) ||
                            x.Description.ToLower().Contains(searchValue)
                    );
                }

                filterRecord = data.Count();

                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    _logger.LogInformation("Sorting by {SortColumn} {SortDirection}", sortColumn, sortColumnDirection);
                    switch (sortColumn)
                    {
                        case "Name":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Name) : data.OrderByDescending(x => x.Name);
                            break;
                        case "Due Date":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                            break;
                        case "GoalStatus":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.GoalStatus) : data.OrderByDescending(x => x.GoalStatus);
                            break;
                        case "Priority":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Priority) : data.OrderByDescending(x => x.Priority);
                            break;
                        case "Status":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.GoalStatus) : data.OrderByDescending(x => x.GoalStatus);
                            break;
                        case "Id":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
                            break;
                        default:
                            data = data.OrderBy(x => x.Id);
                            break;
                    }
                }

                var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();

                var returnObj = new
                {
                    draw,
                    recordsTotal = totalRecord,
                    recordsFiltered = filterRecord,
                    data = _mapper.Map<List<GoalDTO>>(datas)
                };

                _logger.LogInformation("Returning {Count} not started goals (filtered: {FilteredCount})", datas.Count, filterRecord);
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetAllNotStartedGoals for user {UserId}", userId);
                return Json(new { status = false, message = "Error occurred while fetching not started goals." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAllEndedGoals()
        {
            _logger.LogInformation("Entered GetAllEndedGoals");
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetAllEndedGoals.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                int totalRecord = 0;
                int filterRecord = 0;
                var draw = Request.Form["draw"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + sortColumnIndex + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
                int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
                int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

                _logger.LogInformation("DataTable params: draw={Draw}, sortColumn={SortColumn}, sortDirection={SortDirection}, searchValue={SearchValue}, pageSize={PageSize}, skip={Skip}",
                    draw, sortColumn, sortColumnDirection, searchValue, pageSize, skip);

                var data = await _goalService.GetAllGoals(userId, Status.Ended);

                totalRecord = data.Count();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(x => x.Name.ToLower().Contains(searchValue) ||
                            x.Id.ToString().Contains(searchValue) ||
                            x.Description.ToLower().Contains(searchValue)
                    );
                }

                filterRecord = data.Count();

                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    _logger.LogInformation("Sorting by {SortColumn} {SortDirection}", sortColumn, sortColumnDirection);
                    switch (sortColumn)
                    {
                        case "Name":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Name) : data.OrderByDescending(x => x.Name);
                            break;
                        case "Due Date":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
                            break;
                        case "GoalStatus":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.GoalStatus) : data.OrderByDescending(x => x.GoalStatus);
                            break;
                        case "Priority":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Priority) : data.OrderByDescending(x => x.Priority);
                            break;
                        case "Status":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.GoalStatus) : data.OrderByDescending(x => x.GoalStatus);
                            break;
                        case "Id":
                            data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Id) : data.OrderByDescending(x => x.Id);
                            break;
                        default:
                            data = data.OrderBy(x => x.Id);
                            break;
                    }
                }

                var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();

                var returnObj = new
                {
                    draw,
                    recordsTotal = totalRecord,
                    recordsFiltered = filterRecord,
                    data = _mapper.Map<List<GoalDTO>>(datas)
                };

                _logger.LogInformation("Returning {Count} ended goals (filtered: {FilteredCount})", datas.Count, filterRecord);
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetAllEndedGoals for user {UserId}", userId);
                return Json(new { status = false, message = "Error occurred while fetching ended goals." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteGoal(int Id)
        {
            _logger.LogInformation("Entered DeleteGoal with Id={Id}", Id);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in DeleteGoal.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _goalService.DeleteGoal(userId, Id);
                _logger.LogInformation("Goal deleted for Id={Id}, userId={UserId}", Id, userId);
                return Json(new { status = true, message = $"Goal with Id: {Id} is deleted!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in DeleteGoal for Id={Id}, userId={UserId}", Id, userId);
                return Json(new { status = false, message = "Error occurred while deleting goal." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeGoalStatus(int Id, [Bind("Id,GoalStatus")] GoalStatusDTO goalUpdate)
        {
            _logger.LogInformation("Entered ChangeGoalStatus with Id={Id}, GoalStatus={GoalStatus}", Id, goalUpdate.GoalStatus);

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in ChangeGoalStatus.");
                return RedirectToAction("Login", "Account");
            }

            if (Id != goalUpdate.Id)
            {
                _logger.LogWarning("Invalid Parameter Id in ChangeGoalStatus. Expected: {ExpectedId}, Received: {ActualId}", goalUpdate.Id, Id);
                return Json(new { status = false, message = "Invalid Parameter Id!" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _goalService.UpdateGoalStatus(userId, goalUpdate.Id, goalUpdate.GoalStatus);
                    _logger.LogInformation("Goal status updated for Id={Id}, userId={UserId}, newStatus={GoalStatus}", Id, userId, goalUpdate.GoalStatus);
                    return Json(new { status = true, message = $"Goal with Id {Id} Status changed to {goalUpdate.GoalStatus.ToString()}!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception in ChangeGoalStatus for Id={Id}, userId={UserId}", Id, userId);
                    return Json(new { status = false, message = "Error occurred while changing goal status." });
                }
            }

            _logger.LogWarning("ModelState invalid in ChangeGoalStatus for Id={Id}.", Id);
            return Json(new { status = false, message = "ModelState is not valid!" });
        }

        [HttpPost]
        public async Task<IActionResult> SearchGoalNameByName(string searchQuery)
        {
            _logger.LogInformation("Entered SearchGoalNameByName with searchQuery={SearchQuery}", searchQuery);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in SearchGoalNameByName.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _goalService.GetGoalNameBySearchQuery(userId, searchQuery);
                _logger.LogInformation("Returning {Count} goals for searchQuery={SearchQuery}, userId={UserId}", result.Count(), searchQuery, userId);
                return Json(new { status = true, message = $"List of Goals with search query : {searchQuery}", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SearchGoalNameByName for searchQuery={SearchQuery}, userId={UserId}", searchQuery, userId);
                return Json(new { status = false, message = "Error occurred while searching goals." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotesByGoal(int goalId, int pageNumber = 1, int pageSize = 5)
        {
            _logger.LogInformation("Entered GetNotesByGoal with goalId={GoalId}, pageNumber={PageNumber}, pageSize={PageSize}", goalId, pageNumber, pageSize);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetNotesByGoal.");
                return Unauthorized();
            }

            try
            {
                var notes = await _noteService.GetAllNotesByGoalId(userId, goalId, Status.All, pageNumber, pageSize);
                return Json(new { status = true, message = "Goal notes loaded", data = notes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetNotesByGoal for goalId={GoalId}, userId={UserId}", goalId, userId);
                return Json(new { status = false, message = "Error loading notes" });
            }
        }
    }
}
