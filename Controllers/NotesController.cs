using Microsoft.AspNetCore.Mvc;

namespace TaskMonitoringApp.Controllers
{
    public class NotesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}
