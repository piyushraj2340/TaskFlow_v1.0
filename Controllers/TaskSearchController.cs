using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskMonitoringApp.Models.Business;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Controllers
{
    [Authorize]
    public class TaskSearchController : Controller
    {
        private readonly ITaskSearchService _service;
        private readonly UserManager<Users> _userManager;

        public TaskSearchController(ITaskSearchService service, UserManager<Users> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        // Example endpoint: GET /tasksearch?query=test&status=1
        [HttpGet]
        public async Task<IActionResult> SearchTasks([FromQuery] string query, Status status = Status.Running)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Using the simpler DTO for this example endpoint
                var tasks = await _service.SearchTasksExcludingRunningTodos(userId, query, status);
                return Ok(new { status = true, data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        // Example endpoint: GET /tasksearch/withgoals?query=test&status=1
        [HttpGet]
        public async Task<IActionResult> SearchTasksWithGoals([FromQuery] string query, Status status = Status.Running)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Using the DTO that includes goal lists
                var tasksWithGoals = await _service.SearchTasksWithGoalsExcludingRunningTodosAsync(userId, query, status);
                return Ok(new { status = true, data = tasksWithGoals });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }
    }
}