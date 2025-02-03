using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskMonitoringApp.Controllers.API
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        [HttpGet]
        public IActionResult getData()
        {
            return Ok("Dashboard Data...");
        }
    }
}
