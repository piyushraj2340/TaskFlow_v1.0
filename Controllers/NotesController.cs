using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Controllers
{
    [Authorize]
    public class NotesController(INotesServices service, UserManager<Users> userManager, ILogger<NotesController> logger) : Controller
    {
        private readonly INotesServices _service = service;
        private readonly UserManager<Users> _userManager = userManager;
        private readonly ILogger<NotesController> _logger = logger;

        // Index supports optional paging parameters (pageNumber, pageSize)
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20)
        {
            _logger.LogInformation("Entered Index action. pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetAllNotesWithGoalAndTask.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var data = await _service.GetAllNotesWithGoalAndTask(userId, Status.All, pageNumber, pageSize);
                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetAllNotesWithGoalAndTask for userId={UserId}.", userId);
                return View();
            }
        }

        public async Task<IActionResult> Details(int Id)
        {
            _logger.LogInformation("Entered Details action with Id={Id}.", Id);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Details.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var noteData = await _service.GetNoteById(userId, Id);
                if (noteData != null)
                {
                    _logger.LogInformation("Fetched note details for Id={Id}, userId={UserId}.", Id, userId);
                    return Json(new { status = true, message = "Notes Details!", data = noteData });
                }

                _logger.LogWarning("Note not found for Id={Id}, userId={UserId}.", Id, userId);
                return Json(new { status = false, message = "Failed to Load!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Details for Id={Id}, userId={UserId}.", Id, userId);
                return Json(new { status = false, message = "An error occurred while fetching note details." });
            }
        }

        [HttpPost]
        [Route("{controller}/{action}/{AddedWithId}/{noteAddWith?}")]
        public async Task<IActionResult> Create(int AddedWithId, NotesAttachedWith noteAddWith, [Bind("Title,Content,Tags,IsPinned")] NoteDTO note)
        {
            _logger.LogInformation("Entered Create action with AddedWithId={AddedWithId}, noteAddWith={NoteAddWith}, note={@Note}.", AddedWithId, noteAddWith, note);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Create.");
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
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

                        default:
                            _logger.LogWarning("Invalid request type in Create. AddedWithId={AddedWithId}, noteAddWith={NoteAddWith}.", AddedWithId, noteAddWith);
                            return Json(new { status = false, message = "Invalid Request Type!" });
                    }

                    _logger.LogInformation("Note created successfully for AddedWithId={AddedWithId}, noteAddWith={NoteAddWith}, userId={UserId}.", AddedWithId, noteAddWith, userId);
                    return Json(new { status = true, message = "Your Notes Saved!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception in Create for AddedWithId={AddedWithId}, noteAddWith={NoteAddWith}, userId={UserId}.", AddedWithId, noteAddWith, userId);
                    return Json(new { status = false, message = "An error occurred while saving the note." });
                }
            }

            _logger.LogWarning("ModelState invalid in Create. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList());
            return Json(new { status = false, message = "Your notes did not save!" });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Tags,IsPinned")] NoteDTO note)
        {
            _logger.LogInformation("Entered Edit action with id={Id}, note={@Note}.", id, note);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Edit.");
                return RedirectToAction("Login", "Account");
            }

            if (id != note.Id)
            {
                _logger.LogWarning("Note ID mismatch in Edit. Expected: {ExpectedId}, Received: {ActualId}.", note.Id, id);
                return Json(new { status = false, message = "NoteId is not valid!" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    note.UserId = userId;
                    await _service.UpdateNotes(userId, note);

                    _logger.LogInformation("Note updated successfully for id={Id}, userId={UserId}.", id, userId);
                    return Json(new { status = true, message = "Your Notes Saved!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception in Edit for id={Id}, userId={UserId}.", id, userId);
                    return Json(new { status = false, message = "An error occurred while updating the note." });
                }
            }

            _logger.LogWarning("ModelState invalid in Edit for id={Id}. Errors: {Errors}", id, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList());
            return Json(new { status = false, message = "Your notes did not save!" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Entered Delete action with id={Id}.", id);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in Delete.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _service.DeleteNotes(userId, id);

                _logger.LogInformation("Note deleted successfully for id={Id}, userId={UserId}.", id, userId);
                return Json(new { status = true, message = "Your Notes Deleted!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Delete for id={Id}, userId={UserId}.", id, userId);
                return Json(new { status = false, message = "An error occurred while deleting the note." });
            }
        }

        // API: returns notes tree including Goal and Task DTOs, supports paging top-level roots
        [HttpGet]
        public async Task<IActionResult> GetAllNotesWithGoalAndTask(Status status = Status.All, int pageNumber = 1, int pageSize = 5)
        {
            _logger.LogInformation("Entered GetAllNotesWithGoalAndTask with status={Status}, pageNumber={PageNumber}, pageSize={PageSize}.", status, pageNumber, pageSize);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetAllNotesWithGoalAndTask.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var data = await _service.GetAllNotesWithGoalAndTask(userId, status, pageNumber, pageSize);
                return Json(new { status = true, message = "Notes loaded.", data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetAllNotesWithGoalAndTask for userId={UserId}.", userId);
                return Json(new { status = false, message = "An error occurred while fetching notes." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetJournalNotesPartial(int pageNumber = 1, int pageSize = 20, string? lastDate = null)
        {
            _logger.LogInformation("Entered GetJournalNotesPartial pageNumber={PageNumber}, pageSize={PageSize}, lastDate={LastDate}", pageNumber, pageSize, lastDate);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated in GetJournalNotesPartial.");
                return Unauthorized();
            }

            try
            {
                var notes = await _service.GetAllNotesWithGoalAndTask(userId, Status.All, pageNumber, pageSize);
                ViewBag.LastDate = lastDate; // optional, used by partial to avoid duplicate header
                // Render partial that contains the same article/partial structure as Index
                return PartialView("~/Views/Notes/_JournalNotesItems.cshtml", notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetJournalNotesPartial for userId={UserId}.", userId);
                return StatusCode(500, "An error occurred while fetching notes.");
            }
        }
    }
}
