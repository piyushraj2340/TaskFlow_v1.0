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
using TaskMonitoringApp.Models.Enums; 


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

        // Updated Bind to include ParentId
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,EndDate,Priority,Description,StartOptionType,StartDate,ParentId")] GoalViewModel goal)
        {
            _logger.LogInformation("Entered Create with Name={Name}, EndDate={EndDate}, Priority={Priority}, StartOptionType={StartOptionType}, StartDate={StartDate}, ParentId={ParentId}", goal.Name, goal.EndDate, goal.Priority, goal.StartOptionType, goal.StartDate, goal.ParentId);

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

            var pinnedNotes = await _noteService.GetPinnedNotes(userId, filterGoalIds: new[] { Id }, includeGoalRelatedTasks: true);
            ViewBag.PinnedNotes = pinnedNotes;

            var notesList = await _noteService.GetAllNotesByGoalId(userId, Id, Status.All, pageNumber: 1, pageSize: 20);
            var subGoal = await _goalService.GetChildGoals(userId, Id);
            var goalWithNoteList = _mapper.Map<GoalWithNotesAndTaskNameListViewModel>(goal);
            goalWithNoteList.NotesLists = notesList;
            goalWithNoteList.TaskProductivity = productivity;
            goalWithNoteList.SubGoals = subGoal;

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

            var goal = await _goalService.GetGoalById(userId, Id);

            if (goal == null)
            {
                _logger.LogWarning("Goal data not found for Id={Id}, userId={UserId}", Id, userId);
                return Json(new { status = false, message = "Goal Data Not Found!" });
            }

            _logger.LogInformation("Goal data found for Id={Id}, userId={UserId}", Id, userId);
            return Json(new { status = true, message = "Goal Data Found!", data = goal });
        }

        // Updated Bind to include ParentId
        [HttpPut]
        public async Task<ActionResult> Edit([Bind("Id,Name,EndDate,GoalStatus,Description,Priority,StartOptionType,StartDate,ParentId")] GoalViewModel goalData)
        {
            _logger.LogInformation("Entered Edit with Id={Id}, Name={Name}, EndDate={EndDate}, GoalStatus={GoalStatus}, Priority={Priority}, StartOptionType={StartOptionType}, StartDate={StartDate}, ParentId={ParentId}", goalData.Id, goalData.Name, goalData.EndDate, goalData.GoalStatus, goalData.Priority, goalData.StartOptionType, goalData.StartDate, goalData.ParentId);

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

        // New Endpoint: Get Root Goals (Level 0)
        [HttpGet]
        public async Task<IActionResult> GetRootGoals(Status status)
        {
            _logger.LogInformation("Entered GetRootGoals with status={Status}", status);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetRootGoals.");
                return Unauthorized(new { status = false, message = "User not authenticated" });
            }

            try
            {
                var goals = await _goalService.GetRootGoalsWithDynamicStatusUpdatesAsync(userId, status);
                return Json(new { status = true, data = goals });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching root goals for user {UserId}", userId);
                return Json(new { status = false, message = "Error fetching goals" });
            }
        }

        // New Endpoint: Get Child Goals (Next Level)
        [HttpGet]
        public async Task<IActionResult> GetChildGoals(int parentId)
        {
            _logger.LogInformation("Entered GetChildGoals with parentId={ParentId}", parentId);
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetChildGoals.");
                return Unauthorized(new { status = false, message = "User not authenticated" });
            }

            try
            {
                var goals = await _goalService.GetChildGoals(userId, parentId);
                return Json(new { status = true, data = goals });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching child goals for parent {ParentId}, user {UserId}", parentId, userId);
                return Json(new { status = false, message = "Error fetching child goals" });
            }
        }

        // Existing DataTables endpoints (kept for backward compatibility if needed, or can be removed if fully replaced)
        [HttpPost]
        public async Task<IActionResult> GetAllRunningGoals()
        {
            // ... (Existing implementation remains unchanged)
            return await FetchGoalsForDataTable(Status.Running);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllCompletedGoals()
        {
            return await FetchGoalsForDataTable(Status.Completed);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllNotStartedGoals()
        {
            return await FetchGoalsForDataTable(Status.NotStarted);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllEndedGoals()
        {
            return await FetchGoalsForDataTable(Status.Ended);
        }

        // Helper method to reduce code duplication in DataTable endpoints
        private async Task<IActionResult> FetchGoalsForDataTable(Status status)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
                int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
                int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

                var data = await _goalService.GetAllGoalsWithDynamicStatusUpdatesAsync(userId, status);
                int totalRecord = data.Count();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(x => x.Name.ToLower().Contains(searchValue) ||
                            x.Id.ToString().Contains(searchValue) ||
                            (x.Description != null && x.Description.ToLower().Contains(searchValue)));
                }

                int filterRecord = data.Count();
                var datas = pageSize > 0 ? data.Skip(skip).Take(pageSize).ToList() : data.ToList();

                return Json(new
                {
                    draw,
                    recordsTotal = totalRecord,
                    recordsFiltered = filterRecord,
                    data = _mapper.Map<List<GoalDTO>>(datas)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching goals for status {Status}", status);
                return Json(new { status = false, message = "Error occurred while fetching goals." });
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

        [HttpGet]
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

        [HttpPost]
        public async Task<IActionResult> DeattachSubGoals([FromBody] List<int> goalIds)
        {
            _logger.LogInformation("Attempting to de-attach goals: {GoalIds}", string.Join(",", goalIds));

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _goalService.DeattachSubGoals(userId, goalIds);
                if (result)
                {
                    return Json(new { status = true, message = "Sub-goals successfully de-attached into root goals." });
                }
                return Json(new { status = false, message = "No valid goal IDs provided." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de-attaching goals for user {UserId}", userId);
                return Json(new { status = false, message = "An error occurred during de-attachment." });
            }
        }
    }
}