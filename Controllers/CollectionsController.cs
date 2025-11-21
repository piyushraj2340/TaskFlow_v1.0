using Microsoft.AspNetCore.Mvc;

namespace TaskMonitoringApp.Controllers
{
    public class CollectionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
