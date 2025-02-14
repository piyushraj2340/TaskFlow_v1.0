using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Services
{
    public interface INotesServices
    {
        Task<IEnumerable<Notes>> GetAllNotesWithFullContext(string userId, Status status);

        Task<IEnumerable<NoteDTO>> GetAllNotes(string userId, Status status);

        Task<IEnumerable<NoteDTOWithGoalDTO>> GetAllNotesByGoalId(string userId, int goalId, Status status);
        
        Task<IEnumerable<NoteDTOWithTaskDTO>> GetAllNotesByTaskId(string userId, int taskId, Status status);

        Task<Notes> GetNoteByIdWithFullContext(string userId, int notesId);

        Task<NoteDTO> GetNoteById(string userId, int notesId);

        Task<NoteDTOWithGoalDTO> GetNoteByIdByGoalId(string userId, int notesId, int goalId);

        Task<NoteDTOWithTaskDTO> GetNoteByIdByTaskId(string userId, int notesId, int taskId);

        Task AddNotesWithGoalId(string userId, NoteDTO notes, int goalId);

        Task AddNotesWithTaskId(string userId, NoteDTO notes, int taskId);

        Task UpdateNotes(string userId, NoteDTO noteToUpdate);

        Task DeleteNotes(string userId, int noteId);
    }
}
