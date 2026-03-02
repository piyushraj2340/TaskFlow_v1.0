using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Controllers
{
    [Authorize]
    public class NotesController(INotesServices service, UserManager<Users> userManager, ILogger<NotesController> logger, ICompositeViewEngine viewEngine) : Controller
    {
        private readonly INotesServices _service = service;
        private readonly UserManager<Users> _userManager = userManager;
        private readonly ILogger<NotesController> _logger = logger;
        private readonly ICompositeViewEngine _viewEngine = viewEngine;

        // Index supports optional paging parameters (pageNumber, pageSize)
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            // Initial load: Get pinned + first page of timeline (unfiltered)
            var pinned = await _service.GetPinnedNotes(userId);
            var timeline = await _service.GetAllNotesWithGoalAndTask(userId, Status.All, 1, 20);
            
            // Pass a viewmodel or ViewBags
            ViewBag.PinnedNotes = pinned;
            return View(timeline);
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

                        case NotesAttachedWith.Todo:
                            // new: add note attached to a todo
                            await _service.AddNotesWithTodoId(userId, note, todoId: AddedWithId);
                            break;

                        case NotesAttachedWith.All:
                            // independent journal note (no parent)
                            await _service.AddNotesIndependent(userId, note);
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
        public async Task<IActionResult> GetJournalNotesPartial(
            int pageNumber = 1, 
            int pageSize = 20, 
            string? lastDate = null,
            [FromQuery] int[]? filterGoalIds = null,
            [FromQuery] int[]? filterTaskIds = null,
            string? searchQuery = null,
            bool includeGoalRelatedTasks = false)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            try
            {
                var notes = await _service.GetAllNotesWithGoalAndTask(userId, Status.All, pageNumber, pageSize, filterGoalIds, filterTaskIds, searchQuery, includeGoalRelatedTasks);
                ViewBag.LastDate = lastDate;
                return PartialView("~/Views/Notes/_JournalNotesItems.cshtml", notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in partial");
                return StatusCode(500);
            }
        }

        // NEW: Specific endpoint for Goal/Task details pages to return the correct layout
        [HttpGet]
        public async Task<IActionResult> GetGoalTaskNotesPartial(
            int pageNumber = 1, 
            int pageSize = 20, 
            string? lastDate = null,
            [FromQuery] int[]? filterGoalIds = null,
            [FromQuery] int[]? filterTaskIds = null,
            string? searchQuery = null,
            bool includeGoalRelatedTasks = false)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            try
            {
                // Reusing the same service method as it handles filtering by Goal/Task IDs correctly
                var notes = await _service.GetAllNotesWithGoalAndTask(userId, Status.All, pageNumber, pageSize, filterGoalIds, filterTaskIds, searchQuery, includeGoalRelatedTasks);
                ViewBag.LastDate = lastDate;
                // Return the new partial that uses NotesLayout
                return PartialView("~/Views/Shared/_GoalTaskNotesItems.cshtml", notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetGoalTaskNotesPartial");
                return StatusCode(500);
            }
        }

        // NEW: Get Filter Options for the UI dropdown
        [HttpGet]
        public async Task<IActionResult> GetFilterMenuOptions()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            // Updated to use the new method inside service
            var data = await _service.GetFilterOptionsWithCounts(userId);
            
            return Json(new { status = true, data });
        }

        // NEW: Endpoint to refresh pinned notes based on filters
        [HttpGet]
        public async Task<IActionResult> GetPinnedNotesPartial(
            [FromQuery] int[]? filterGoalIds = null,
            [FromQuery] int[]? filterTaskIds = null,
            string? searchQuery = null,
            bool includeGoalRelatedTasks = false)
        {
             var userId = _userManager.GetUserId(User);
             if (userId == null) return Unauthorized();

             var pinned = await _service.GetPinnedNotes(userId, filterGoalIds, filterTaskIds, searchQuery, includeGoalRelatedTasks);
             return PartialView("~/Views/Notes/_PinnedNotesList.cshtml", pinned);
        }

        // NEW: Endpoint to fetch a single note partial for "Jump to Note" feature
        [HttpGet]
        public async Task<IActionResult> GetSingleNotePartial(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            try
            {
                // We reuse the existing service logic but just for one ID.
                // Assuming you have implemented GetNoteByIdWithGoalAndTask in Service as defined below in step 4
                var note = await _service.GetNoteByIdWithGoalAndTask(userId, id);

                if (note == null) return NotFound();

                // Create a list of 1 to reuse the existing partial view
                var list = new List<NoteDTOWithGoalAndTaskDTO> { note };
                return PartialView("~/Views/Notes/_JournalNotesItems.cshtml", list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching single note partial for id={Id}", id);
                return StatusCode(500);
            }
        }

        // UPDATED: Return JSON with HTML string and metadata
        [HttpGet]
        public async Task<IActionResult> GetPageForNote(int noteId, int pageSize = 20, [FromQuery] int[]? filterGoalIds = null, [FromQuery] int[]? filterTaskIds = null, string? searchQuery = null, bool includeGoalRelatedTasks = false)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            // 1. Calculate page
            var result = await _service.GetNotePageAndContext(userId, noteId, pageSize, Status.All, filterGoalIds, filterTaskIds, searchQuery, includeGoalRelatedTasks);
            
            if (result.Note == null) return NotFound(new { message = "Note not found in filter" });

            // 2. Fetch page data
            var notesOnPage = await _service.GetAllNotesWithGoalAndTask(userId, Status.All, result.PageNumber, pageSize, filterGoalIds, filterTaskIds, searchQuery, includeGoalRelatedTasks);

            // 3. Render Partial to String
            // Determine which partial to use based on context (if filters are present, we assume specific view)
            string partialViewName = "~/Views/Notes/_JournalNotesItems.cshtml";

            // If we are filtering by specific goals or tasks (Detail Pages context usually), use the GoalTask layout
            if ((filterGoalIds != null && filterGoalIds.Length > 0) || (filterTaskIds != null && filterTaskIds.Length > 0))
            {
                partialViewName = "~/Views/Shared/_GoalTaskNotesItems.cshtml";
            }

            var html = await RenderViewToStringAsync(partialViewName, notesOnPage);

            // 4. Return JSON
            return Json(new { 
                status = true, 
                html = html, 
                pageNumber = result.PageNumber 
            });
        }
        
        // Helper method to render a View/PartialView to a string
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.GetView(null, viewName, false);

                if (viewResult.View == null)
                {
                    viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                }

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}
