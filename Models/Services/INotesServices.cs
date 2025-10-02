using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Services
{
    public interface INotesServices
    {
        Task<IEnumerable<Notes>> GetAllNotesWithFullContext(string userId, Status status);

        // existing - non-paged
        Task<IEnumerable<NoteDTO>> GetAllNotes(string userId, Status status);

        // new - paged
        Task<IEnumerable<NoteDTO>> GetAllNotes(string userId, Status status, int pageNumber, int pageSize);

        Task<IEnumerable<NoteDTOWithGoalDTO>> GetAllNotesByGoalId(string userId, int goalId, Status status);

        // new - paged
        Task<IEnumerable<NoteDTOWithGoalDTO>> GetAllNotesByGoalId(string userId, int goalId, Status status, int pageNumber, int pageSize);
        
        Task<IEnumerable<NoteDTOWithTaskDTO>> GetAllNotesByTaskId(string userId, int taskId, Status status);

        // new - paged
        Task<IEnumerable<NoteDTOWithTaskDTO>> GetAllNotesByTaskId(string userId, int taskId, Status status, int pageNumber, int pageSize);

        Task<Notes> GetNoteByIdWithFullContext(string userId, int notesId);

        Task<NoteDTO> GetNoteById(string userId, int notesId);

        Task<NoteDTOWithGoalDTO> GetNoteByIdByGoalId(string userId, int notesId, int goalId);

        Task<NoteDTOWithTaskDTO> GetNoteByIdByTaskId(string userId, int notesId, int taskId);

        Task AddNotesWithGoalId(string userId, NoteDTO notes, int goalId);

        Task AddNotesWithTaskId(string userId, NoteDTO notes, int taskId);

        Task AddNotesWithTodoId(string userId, NoteDTO notes, int todoId);

        Task AddNotesIndependent(string userId, NoteDTO notes);

        Task UpdateNotes(string userId, NoteDTO noteToUpdate);

        Task DeleteNotes(string userId, int noteId);

        // New: returns a parent-child tree of notes with Goal and Task included
        Task<IEnumerable<NoteDTOWithGoalAndTaskDTO>> GetAllNotesWithGoalAndTask(string userId, Status status);

        // New: paged variant (service will build tree then page top-level roots to preserve parent-child integrity)
        Task<IEnumerable<NoteDTOWithGoalAndTaskDTO>> GetAllNotesWithGoalAndTask(string userId, Status status, int pageNumber, int pageSize);
    }
}
