using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
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

        public async Task<IActionResult> Details(int Id)
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var noteData = await _service.GetNoteById(userId, Id);

            if (noteData != null)
            {
                return Json(new { status = true, message = "Notes Details!", data = noteData });
            }

            return Json(new { status = false, message = "Faild to Load!" });

        }

        [HttpPost]
        [Route("{controller}/{action}/{AddedWithId}/{noteAddWith?}")]
        public async Task<IActionResult> Create(int AddedWithId, NotesAttachedWith noteAddWith, [Bind("Title,Content,Tags,IsPinned")] NoteDTO note)
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
                switch (noteAddWith)
                {
                    case NotesAttachedWith.Goal:
                        await _service.AddNotesWithGoalId(userId, note, goalId: AddedWithId);
                        break;

                    case NotesAttachedWith.Task:
                        await _service.AddNotesWithTaskId(userId, note, taskId: AddedWithId);
                        break;

                    //case NotesAttachedWith.Todo:
                    //    await _service.AddNotesWithGoalId(userId, note, goalId: Id);
                    //    break;

                    default:
                        //await _service.AddNotes(userId, note, goalId: Id);
                        return Json(new { status = false, message = "Invalid Request Type!" });
                }

                return Json(new { status = true, message = "Your Notes Saved!" });
            }

            return Json(new { status = false, message = "Your notes does not save!" });

        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Tags,IsPinned")] NoteDTO note)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != note.Id)
            {
                Json(new { status = false, message = "NoteId is not valid!" });
            }

            if (ModelState.IsValid)
            {
                note.UserId = userId;

                await _service.UpdateNotes(userId, note);

                return Json(new { status = true, message = "Your Notes Saved!" });
            }

            return Json(new { status = false, message = "Your notes does not save!" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }


            if (ModelState.IsValid)
            {

                await _service.DeleteNotes(userId, id);

                return Json(new { status = true, message = "Your Notes Saved!" });
            }

            return Json(new { status = false, message = "Your notes does not save!" });
        }
    }
}
