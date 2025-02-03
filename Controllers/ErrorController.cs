using Microsoft.AspNetCore.Mvc;

namespace TaskMonitoringApp.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {
        [HttpGet]
        public IActionResult Index(int statusCode, string message)
        {
            ViewData["StatusCode"] = statusCode;
            ViewData["Message"] = message;

            // Render a user-friendly error view
            return View("Error");
        }
    }
}
