using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Exceptions;
using TaskMonitoringApp.Models;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Controllers.API
{
    [Route("api/v1/Goals")]
    [ApiController]
    [Authorize]
    public class GoalApiController : ControllerBase
    {
        private readonly IGoalServices _service;
        private readonly ILogger<GoalApiController> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<Users> _userManager;

        public GoalApiController(IGoalServices service, ILogger<GoalApiController> logger, IMapper mapper, UserManager<Users> userManager)
        {
            _service = service;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,EndDate,Priority,Description")] GoalDTO goals)
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (goals == null)
            {
                return BadRequest(new ApiResponseModel<string>("Request body is missing or invalid"));
            }

            if (ModelState.IsValid)
            {
                // Log before adding the product
                _logger.LogInformation("Attempting to add a new Goal: {GoalName}", goals.Name);

                await _service.AddNewGoal(userId, goals);

                // Log success after adding the product
                _logger.LogInformation("Goal '{GoalName}' successfully added with ID: {GoalID}", goals.Name, goals.Id);

                if (goals == null)
                {
                    return BadRequest(new ApiResponseModel<string>("Request body is missing or invalid"));
                }

                return CreatedAtAction(nameof(Create), new { id = goals.Id }, new ApiResponseModel<GoalDTO>(true, "Data created successfully", goals));

            }

            return BadRequest(new ApiResponseModel<string>("Model validation failed"));

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int Id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var goal = await _service.GetGoalById(userId, Id);

            if (goal == null)
            {
                // Log the error (optional: log it to a file, database, or monitoring tool)
                _logger.LogInformation("Goal Not Found with {GoalId}", Id);

                return NotFound(new ApiResponseModel<string>("Goal Not Found!."));
            }

            GoalDTO goalDetails = _mapper.Map<GoalDTO>(goal);

            return Ok(new ApiResponseModel<GoalDTO>(true, "Goal Details", goalDetails));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGoals()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var getGoals = await _service.GetAllGoalsWithDynamicStatusUpdatesAsync(userId, Status.All);

            if (getGoals == null)
            {
                return NotFound(new ApiResponseModel<string>("No Active Goal Found!..."));
            }

            var getAllGoals = _mapper.Map<List<GoalDTO>>(getGoals);

            return Ok(new ApiResponseModel<List<GoalDTO>>(true, "List of All Active Goal!..", getAllGoals));
        }
    }
}
