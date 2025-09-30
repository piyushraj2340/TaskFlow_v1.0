using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Controllers
{
    [Authorize]
    public class SearchController(ISearchServices searchService, UserManager<Users> userManager, ILogger<SearchController> logger) : Controller
    {
        private readonly ISearchServices _searchService = searchService;
        private readonly UserManager<Users> _userManager = userManager;
        private readonly ILogger<SearchController> _logger = logger;

        // GET: /Search/Query?q=...&pageNumber=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> Query(string q, int pageNumber = 1, int pageSize = 20)
        {
            _logger.LogInformation("Entered Search.Query q={Query} pageNumber={PageNumber} pageSize={PageSize}", q, pageNumber, pageSize);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Search.Query.");
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new { status = true, data = Array.Empty<SearchResultDTO>() });
            }

            try
            {
                var results = await _searchService.Search(userId, q, pageNumber, pageSize);
                return Json(new { status = true, data = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Search.Query for userId={UserId}, query={Query}", userId, q);
                return Json(new { status = false, message = "An error occurred while searching." });
            }
        }
    }
}