using AutoMapper;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Models.Business
{
    public class NotesServices(IMapper mapper, INotesRepository repository, ApplicationDbContext context) : INotesServices // Added context direct access for lightweight filter fetching or inject specific repos
    {
        // Note: Ideally inject IGoalRepository/ITaskRepository, but using Context here to keep it short for the "Filter Menu" logic
        private readonly ApplicationDbContext _context = context; 
        private readonly INotesRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task AddNotesWithGoalId(string userId, NoteDTO notes, int goalId)
        {
            if(userId != notes.UserId)
            {
                throw new InvalidOperationException("userId and notes.UserId must be same!");
            }

            var notesToAdd = _mapper.Map<Notes>(notes);
            notesToAdd.GoalId = goalId;
            notesToAdd.TimeStamp = DateTime.Now;

            await _repository.AddNotes(notesToAdd);
        }

        public async Task AddNotesWithTaskId(string userId, NoteDTO notes, int taskId)
        {
            if (userId != notes.UserId)
            {
                throw new InvalidOperationException("userId and notes.UserId must be same!");
            }

            var notesToAdd = _mapper.Map<Notes>(notes);
            notesToAdd.TaskId = taskId;
            notesToAdd.TimeStamp = DateTime.Now;

            await _repository.AddNotes(notesToAdd);
        }

        // existing non-paged method
        public async Task<IEnumerable<NoteDTO>> GetAllNotes(string userId, Status status)
        {
            return await _repository.GetAllNotesAsync<NoteDTO>(userId, status, ResponseDataMode.ModelDTO);
        }

        // new paged service method - uses repository paged overload
        public async Task<IEnumerable<NoteDTO>> GetAllNotes(string userId, Status status, int pageNumber, int pageSize)
        {
            return await _repository.GetAllNotesAsync<NoteDTO>(userId, status, pageNumber, pageSize, ResponseDataMode.ModelDTO);
        }

        public async Task<IEnumerable<NoteDTOWithGoalDTO>> GetAllNotesByGoalId(string userId, int goalId, Status status)
        {
            return await _repository.GetAllNotesForGoalIdAsync<NoteDTOWithGoalDTO>(userId, goalId, status, ResponseDataMode.ModelDTO); 
        }

        // new paged service method for goal notes
        public async Task<IEnumerable<NoteDTOWithGoalDTO>> GetAllNotesByGoalId(string userId, int goalId, Status status, int pageNumber, int pageSize)
        {
            return await _repository.GetAllNotesForGoalIdAsync<NoteDTOWithGoalDTO>(userId, goalId, status, pageNumber, pageSize, ResponseDataMode.ModelDTO);
        }

        public async Task<IEnumerable<NoteDTOWithTaskDTO>> GetAllNotesByTaskId(string userId, int taskId, Status status)
        {
            return await _repository.GetAllNotesForTaskIdAsync<NoteDTOWithTaskDTO>(userId, taskId, status, ResponseDataMode.ModelDTO); 
        }

        // new paged service method for task notes
        public async Task<IEnumerable<NoteDTOWithTaskDTO>> GetAllNotesByTaskId(string userId, int taskId, Status status, int pageNumber, int pageSize)
        {
            return await _repository.GetAllNotesForTaskIdAsync<NoteDTOWithTaskDTO>(userId, taskId, status, pageNumber, pageSize, ResponseDataMode.ModelDTO);
        }

        public async Task<IEnumerable<Notes>> GetAllNotesWithFullContext(string userId, Status status)
        {
            return await _repository.GetAllNotesAsync<Notes>(userId, status, ResponseDataMode.Model);
        }

        public async Task<NoteDTO> GetNoteById(string userId, int noteId)
        {
            return await _repository.GetNotesByIdAsync<NoteDTO>(userId, noteId, ResponseDataMode.ModelDTO);
        }

        public async Task<NoteDTOWithGoalDTO> GetNoteByIdByGoalId(string userId, int noteId, int goalId)
        {
            return await _repository.GetNotesByIdWithGoalIdAsync<NoteDTOWithGoalDTO>(userId, noteId, goalId, ResponseDataMode.ModelDTO);
        }

        public async Task<NoteDTOWithTaskDTO> GetNoteByIdByTaskId(string userId, int noteId, int taskId)
        {
            return await _repository.GetNotesByIdWithTaskIdAsync<NoteDTOWithTaskDTO>(userId, noteId, taskId, ResponseDataMode.ModelDTO);
        }

        public async Task<Notes> GetNoteByIdWithFullContext(string userId, int noteId)
        {
            return await _repository.GetNotesByIdAsync<Notes>(userId, noteId, ResponseDataMode.Model);
        }

        public async Task UpdateNotes(string userId, NoteDTO noteToUpdate)
        {
            var getNotes = await _repository.GetNotesByIdAsync<Notes>(userId, noteToUpdate.Id, ResponseDataMode.Model);

            if(noteToUpdate.Title != getNotes.Title || noteToUpdate.Content != getNotes.Content)
            {
                noteToUpdate.IsModified = true;
                noteToUpdate.ModifiedOn = DateTime.Now;
            }

            noteToUpdate.TimeStamp = getNotes.TimeStamp; // Do Not change the timestamp if we update the notes....

            _mapper.Map<NoteDTO, Notes>(noteToUpdate, getNotes);

            await _repository.UpdateNotes(userId, getNotes);
        }

        public async Task DeleteNotes(string userId, int noteId)
        {
            var noteData = await _repository.GetNotesByIdAsync<Notes>(userId, noteId, ResponseDataMode.Model);

            noteData.IsDeleted = true;
            noteData.DeletedOn = DateTime.Now;
            noteData.UpdatedOn = DateTime.Now;

            await _repository.UpdateNotes(userId, noteData);
            
        }

        // New: returns a parent-child tree of notes including Goal & Task DTOs
        public async Task<IEnumerable<NoteDTOWithGoalAndTaskDTO>> GetAllNotesWithGoalAndTask(string userId, Status status)
        {
            // fetch full flat list (no paging at repository level to keep parent/child integrity)
            var flatList = await _repository.GetAllNotesWithGoalAndTaskAsync<NoteDTOWithGoalAndTaskDTO>(userId, status, ResponseDataMode.ModelDTO);

            // Build dictionary for quick lookups
            var dict = flatList.ToDictionary(n => n.Id, n => n);

            // Ensure children lists are mutable (should be by DTO default)
            foreach (var node in dict.Values)
            {
                if (node.Children == null)
                    node.Children = new List<NoteDTOWithGoalAndTaskDTO>();
            }

            var roots = new List<NoteDTOWithGoalAndTaskDTO>();

            foreach (var node in dict.Values.OrderByDescending(n => n.TimeStamp))
            {
                if (node.ParentNoteId.HasValue && dict.TryGetValue(node.ParentNoteId.Value, out var parent))
                {
                    parent.Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }

            return roots;
        }

        // New: paged variant — build full tree and then paginate top-level roots to preserve parent-child relationships.
        public async Task<IEnumerable<NoteDTOWithGoalAndTaskDTO>> GetAllNotesWithGoalAndTask(string userId, Status status, int pageNumber, int pageSize)
        {
            var allRoots = (await GetAllNotesWithGoalAndTask(userId, status)).ToList();

            if (pageNumber <= 0 || pageSize <= 0)
                return allRoots;

            var pagedRoots = allRoots
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedRoots;
        }

        // new - create note attached to a Todo
        public async Task AddNotesWithTodoId(string userId, NoteDTO notes, int todoId)
        {
            if (userId != notes.UserId)
                throw new InvalidOperationException("userId and notes.UserId must be same!");

            var notesToAdd = _mapper.Map<Notes>(notes);
            notesToAdd.TodoId = todoId;
            notesToAdd.TimeStamp = DateTime.Now;

            await _repository.AddNotes(notesToAdd);
        }

        // new - independent note
        public async Task AddNotesIndependent(string userId, NoteDTO notes)
        {
            if (userId != notes.UserId)
                throw new InvalidOperationException("userId and notes.UserId must be same!");

            var notesToAdd = _mapper.Map<Notes>(notes);
            notesToAdd.TimeStamp = DateTime.Now;
            // no parent ids set

            await _repository.AddNotes(notesToAdd);
        }

        // MODIFIED
        public async Task<IEnumerable<NoteDTOWithGoalAndTaskDTO>> GetAllNotesWithGoalAndTask(
            string userId, 
            Status status, 
            int pageNumber, 
            int pageSize,
            IEnumerable<int>? filterGoalIds = null, 
            IEnumerable<int>? filterTaskIds = null, 
            string? searchQuery = null,
            bool includeGoalRelatedTasks = false)
        {
            // Note: We are now passing filters to repository directly
            var roots = await _repository.GetAllNotesWithGoalAndTaskAsync<NoteDTOWithGoalAndTaskDTO>(
                userId, 
                status, 
                pageNumber, 
                pageSize, 
                ResponseDataMode.ModelDTO, 
                filterGoalIds, 
                filterTaskIds, 
                searchQuery,
                onlyPinned: false, // Don't filter only pinned here, we want timeline
                includeGoalRelatedTasks: includeGoalRelatedTasks
            );

            return roots;
        }

        // NEW
        public async Task<IEnumerable<NoteDTOWithGoalAndTaskDTO>> GetPinnedNotes(
             string userId, 
             IEnumerable<int>? filterGoalIds = null, 
             IEnumerable<int>? filterTaskIds = null, 
             string? searchQuery = null,
             bool includeGoalRelatedTasks = false)
        {
            // Fetch only pinned notes
            return await _repository.GetAllNotesWithGoalAndTaskAsync<NoteDTOWithGoalAndTaskDTO>(
                userId, 
                Status.All, 
                pageNumber: 1, 
                pageSize: 1000, // Reasonable limit for pinned items
                ResponseDataMode.ModelDTO, 
                filterGoalIds, 
                filterTaskIds, 
                searchQuery,
                onlyPinned: true,
                includeGoalRelatedTasks: includeGoalRelatedTasks
            );
        }

        // NEW
        public async Task<FilterMenuDataDTO> GetFilterOptionsWithCounts(string userId)
        {
            return await _repository.GetFilterOptionsWithCountsAsync(userId);
        }

        public async Task<NoteDTOWithGoalAndTaskDTO> GetNoteByIdWithGoalAndTask(string userId, int noteId)
        {
            // NEW: Fetch specific note with formatting
            return await _repository.GetNoteByIdWithGoalAndTaskAsync<NoteDTOWithGoalAndTaskDTO>(userId, noteId, ResponseDataMode.ModelDTO);
        }

        public async Task<(int PageNumber, NoteDTOWithGoalAndTaskDTO Note)> GetNotePageAndContext(string userId, int noteId, int pageSize, Status status, IEnumerable<int>? filterGoalIds = null, IEnumerable<int>? filterTaskIds = null, string? searchQuery = null, bool includeGoalRelatedTasks = false)
        {
            if (pageSize <= 0) pageSize = 20;

            // 1. Get Position
            int position = await _repository.GetNotePositionAsync(userId, noteId, status, filterGoalIds, filterTaskIds, searchQuery, includeGoalRelatedTasks);
            
            if (position == -1) return (0, null);

            // 2. Calculate Page: Ceiling(Position / PageSize)
            int pageNumber = (int)Math.Ceiling((double)position / pageSize);

            // 3. Fetch the actual note data to return context
            var note = await _repository.GetNoteByIdWithGoalAndTaskAsync<NoteDTOWithGoalAndTaskDTO>(userId, noteId, ResponseDataMode.ModelDTO);

            return (pageNumber, note);
        }
    }
}
