using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Controllers
{
    [Authorize]
    public class NotesController(INotesServices service, UserManager<Users> userManager) : Controller
    {
        private readonly INotesServices _service = service;
        private readonly UserManager<Users> _userManager = userManager;
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int Id, [Bind("Title,Content,Tags")]NoteDTO note)
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                note.UserId = userId;
                await _service.AddNotesWithGoalId(userId, note, Id);

                return Json(new { status = true, message = "Your Notes Saved!" });
            }

            return Json(new { status = false, message = "Your data does't save!" });

        }
    }
}
