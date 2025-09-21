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
        private readonly IGoalServices _goalService;
        private readonly INotesServices _noteService;
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<Users> _userManager;

        public GoalsController(ILogger<HomeController> logger, IGoalServices goalService, INotesServices noteService, IMapper mapper, UserManager<Users> userManager)
        {
            _goalService = goalService;
            _noteService = noteService;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            GoalProductivityDTO productivity = await _goalService.GetGoalProductivity(userId);

            return View(productivity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,EndDate,Priority,Description,StartOptionType,StartDate")] GoalViewModel goal)
        {

            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            goal.UserId = userId;

            if (goal.StartOptionType == StartOptions.Scheduled && goal.StartDate == null)
            {
                ModelState.AddModelError("", "Start Date Must be required for scheduled!");
            }

            if (ModelState.IsValid)
            {
                // Log before adding the product
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

                // Log success after adding the product
                _logger.LogInformation("Goal '{GoalName}' successfully added with ID: {GoalID}", goal.Name, goal.Id);

                return Json(new { status = true, message = "New Goal Successfully Added", data = goal });
            }

            // If the model is invalid, return failure response
            return Json(new { status = false, message = "ModelState is not valid!", ErrormessageList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
        }

        [Route("Goals/Details/{Id}/{tabName?}")]
        public async Task<IActionResult> Details(int Id, string? tabName)
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var goal = await _goalService.GetAllTaskNameWithStatusAndGoal(userId, Id, Status.All);

            if (goal == null)
            {
                return NotFound();
            }


            ViewBag.tabName = tabName;

            var notesList = await _noteService.GetAllNotesByGoalId(userId, Id, Status.All);
            var goalWithNoteList = _mapper.Map<GoalWithNotesAndTaskNameListViewModel>(goal);
            goalWithNoteList.NotesLists = notesList;


            return View(goalWithNoteList);
            // TODO: Create a view model for the notes and GoalWithTaskNameList
            //return View(_mapper.Map<GoalWithTaskNameListViewModel>(goal));
        }

        public async Task<IActionResult> GetById(int Id)
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var goal = await _goalService.GetAllTaskNameWithStatusAndGoal(userId, Id, Status.All);

            if (goal == null)
            {
                return Json(new { status = false, message = "Goal Data Not Found!" });
            }

            return Json(new { status = true, message = "Goal Data Found!", data = goal });
        }

        [HttpPut]
        public async Task<IActionResult> Edit([Bind("Id,Name,EndDate,GoalStatus,Description,Priority,StartOptionType,StartDate")] GoalViewModel goalData)
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (goalData.StartOptionType == StartOptions.Scheduled && goalData.StartDate == null)
            {
                ModelState.AddModelError("", "Start Date Must be required for scheduled!");
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

                return Json(new { status = true, message = "Goal updated successfully!" });
            }
            return Json(new { status = false, message = "ModelState Invalid!" });
        }

        [HttpPost]
        public async Task<IActionResult> GetAllRunningGoals()
        {
            // Get the logged-in user's ID
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

            // Get all goals from the goalService
            var data = await _goalService.GetAllGoals(userId, Status.Running);

            // Get total count of records
            totalRecord = data.Count();


            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.Name.ToLower().Contains(searchValue) || 
                        x.Id.ToString().Contains(searchValue) || 
                        x.Description.ToLower().Contains(searchValue)
                );
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
                data = _mapper.Map<List<GoalDTO>>(empList)
            };

            // Return the result as JSON
            return Json(returnObj);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllCompletedGoals()
        {
            // Get the logged-in user's ID
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

            // Get all goals from the goalService
            var data = await _goalService.GetAllGoals(userId, Status.Completed);

            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.Name.ToLower().Contains(searchValue) ||
                        x.Id.ToString().Contains(searchValue) ||
                        x.Description.ToLower().Contains(searchValue)
                );
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
                data = _mapper.Map<List<GoalDTO>>(empList)
            };

            // Return the result as JSON
            return Json(returnObj);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllNotStartedGoals()
        {
            // Get the logged-in user's ID
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

            // Get all goals from the goalService
            var data = await _goalService.GetAllGoals(userId, Status.NotStarted);

            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.Name.ToLower().Contains(searchValue) ||
                        x.Id.ToString().Contains(searchValue) ||
                        x.Description.ToLower().Contains(searchValue)
                );
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
                data = _mapper.Map<List<GoalDTO>>(empList)
            };

            // Return the result as JSON
            return Json(returnObj);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllEndedGoals()
        {
            // Get the logged-in user's ID
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
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower().ToLower();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            // Get all goals from the goalService
            var data = await _goalService.GetAllGoals(userId, Status.Ended);

            // Get total count of records
            totalRecord = data.Count();

            // Apply search filter if there's a search value
            if (!string.IsNullOrEmpty(searchValue))
            {
                // Perform case-insensitive search on multiple fields (Name and Id)
                data = data.Where(x => x.Name.ToLower().Contains(searchValue) ||
                        x.Id.ToString().Contains(searchValue) ||
                        x.Description.ToLower().Contains(searchValue)
                );
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
                data = _mapper.Map<List<GoalDTO>>(empList)
            };

            // Return the result as JSON
            return Json(returnObj);
        }

        //[HttpPost]
        //public async Task<IActionResult> GetAllDeletedGoals()
        //{
        //    // Get the logged-in user's ID
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

        //    // Get all goals from the goalService
        //    var data = await _goalService.GetAllGoals(userId, Status.Deleted);

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
        //            case "Due Date":
        //                data = sortColumnDirection == "asc" ? data.OrderBy(x => x.EndDate) : data.OrderByDescending(x => x.EndDate);
        //                break;
        //            case "GoalStatus":
        //                data = sortColumnDirection == "asc" ? data.OrderBy(x => x.GoalStatus) : data.OrderByDescending(x => x.GoalStatus);
        //                break;
        //            case "Priority":
        //                data = sortColumnDirection == "asc" ? data.OrderBy(x => x.Priority) : data.OrderByDescending(x => x.Priority);
        //                break;
        //            case "Status":
        //                data = sortColumnDirection == "asc" ? data.OrderBy(x => x.GoalStatus) : data.OrderByDescending(x => x.GoalStatus);
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

        //    // Map the data to GoalDTO using AutoMapper
        //    var returnObj = new
        //    {
        //        draw,
        //        recordsTotal = totalRecord,
        //        recordsFiltered = filterRecord,
        //        data = _mapper.Map<List<GoalDTO>>(empList)
        //    };

        //    // Return the result as JSON
        //    return Json(returnObj);
        //}

        [HttpDelete]
        public async Task<IActionResult> DeleteGoal(int Id)
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await _goalService.DeleteGoal(userId, Id);
            return Json(new { status = true, message = $"Goal with Id: {Id} is deleted!" });

        }

        [HttpPost]
        public async Task<IActionResult> ChangeGoalStatus(int Id, [Bind("Id,GoalStatus")] GoalStatusDTO goalUpdate)
        {

            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Id != goalUpdate.Id) return Json(new { status = false, message = "Invalid Parameter Id!" });

            if (ModelState.IsValid)
            {
                await _goalService.UpdateGoalStatus(userId, goalUpdate.Id, goalUpdate.GoalStatus);
                return Json(new { status = true, message = $"Goal with Id {Id} Status changed to {goalUpdate.GoalStatus.ToString()}!" });
            }

            return Json(new { status = false, message = "ModelState is not valid!" });
        }

        [HttpPost]
        public async Task<IActionResult> SearchGoalNameByName(string searchQuery)
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _goalService.GetGoalNameBySearchQuery(userId, searchQuery);

            return Json(new { status = true, message = $"List of Goals with search query : {searchQuery}", data = result });
        }

        //public async Task<IActionResult> GetProductivity()
        //{
        //    int runningGoal = await _goalService.GetGoalCountByGoalStatus(Status.Running);

        //    // over all productivity...
        //    int endGoalCount = await _goalService.GetGoalCountByGoalStatus(Status.Ended);
        //    int completedGoalCount = await _goalService.GetGoalCountByGoalStatus(Status.Completed);

        //    // Ensure that the division happens with floating-point precision
        //    double productivity = (((double)completedGoalCount / (endGoalCount + completedGoalCount)) * 100);

        //    // Round to 2 decimal places
        //    productivity = Math.Round(productivity, 2);




        //    // Calculating the previous week productivity....
        //    int endGoalPreviousWeekCount = await _goalService.GetGoalCountByGoalStatusAndDateTimeRange(Status.Ended, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));
        //    int completedPreviousWeekGoalCount = await _goalService.GetGoalCountByGoalStatusAndDateTimeRange(Status.Completed, DateTime.Now.AddDays(-14), DateTime.Now.AddDays(-7));

        //    // Ensure that the division happens with floating-point precision
        //    double productivityPreviousWeek = (((double)completedPreviousWeekGoalCount / (endGoalPreviousWeekCount + completedPreviousWeekGoalCount)) * 100);

        //    // Round to 2 decimal places
        //    productivityPreviousWeek = Math.Round(productivityPreviousWeek, 2);

        //    // if the completed and ended task is zero then 0 productivity...
        //    if (endGoalPreviousWeekCount + completedPreviousWeekGoalCount == 0)
        //    {
        //        productivityPreviousWeek = 0;
        //    }


        //    // Calculating the current week productivity....
        //    int endGoalCurrentWeekCount = await _goalService.GetGoalCountByGoalStatusAndDateTimeRange(Status.Ended, DateTime.Now.AddDays(-7), DateTime.Now);
        //    int completedCurrentWeekGoalCount = await _goalService.GetGoalCountByGoalStatusAndDateTimeRange(Status.Completed, DateTime.Now.AddDays(-7), DateTime.Now);

        //    // Ensure that the division happens with floating-point precision
        //    double productivityCurrent = (((double)completedCurrentWeekGoalCount / (endGoalCurrentWeekCount + completedCurrentWeekGoalCount)) * 100);

        //    // Round to 2 decimal places
        //    productivityCurrent = Math.Round(productivityCurrent, 2);

        //    // if the completed and ended task is zero then 0 productivity...
        //    if (endGoalCurrentWeekCount + completedCurrentWeekGoalCount == 0)
        //    {
        //        productivityCurrent = 0;
        //    }

        //    double growthPercentage = ((double)((productivityCurrent - productivityPreviousWeek) / productivityPreviousWeek) * 100);


        //    // Round to 2 decimal places
        //    growthPercentage = Math.Round(growthPercentage, 2);


        //    // if the previous groth percentage is 0 then 100% groth..
        //    if (productivityPreviousWeek == 0)
        //    {
        //        growthPercentage = 100.00d;
        //    }


        //    return Json(new
        //    {
        //        status = true,
        //        message = "Overall Goal Productivity!.",
        //        data = new
        //        {
        //            productivity,
        //            runningGoal,
        //            completedGoal = completedGoalCount,
        //            growthPercentage
        //        }
        //    });
        //}
    }
}
