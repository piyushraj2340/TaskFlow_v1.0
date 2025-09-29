using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface INotesRepository
    {
        // existing - non-paged
        Task<IEnumerable<T>> GetAllNotesAsync<T>(string userId, Status status, ResponseDataMode mode) where T : class;

        // new - paged overload
        Task<IEnumerable<T>> GetAllNotesAsync<T>(string userId, Status status, int pageNumber, int pageSize, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllNotesForGoalIdAsync<T>(string userId, int goalId, Status status, ResponseDataMode mode) where T : class;

        // New: paged overload for goal-specific notes
        Task<IEnumerable<T>> GetAllNotesForGoalIdAsync<T>(string userId, int goalId, Status status, int pageNumber, int pageSize, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllNotesForTaskIdAsync<T>(string userId, int taskId, Status status, ResponseDataMode mode) where T : class;

        // New: paged overload for task-specific notes
        Task<IEnumerable<T>> GetAllNotesForTaskIdAsync<T>(string userId, int taskId, Status status, int pageNumber, int pageSize, ResponseDataMode mode) where T : class;

        // New: fetch notes including both Goal and Task data (DTO) or full model
        Task<IEnumerable<T>> GetAllNotesWithGoalAndTaskAsync<T>(string userId, Status status, ResponseDataMode mode) where T : class;

        // New: paged overload for WithGoalAndTask
        Task<IEnumerable<T>> GetAllNotesWithGoalAndTaskAsync<T>(string userId, Status status, int pageNumber, int pageSize, ResponseDataMode mode) where T : class;

        Task<T> GetNotesByIdAsync<T>(string userId, int id, ResponseDataMode mode) where T : class;

        Task<T> GetNotesByIdWithGoalIdAsync<T>(string userId, int id, int goalId, ResponseDataMode mode)where T : class;

        Task<T> GetNotesByIdWithTaskIdAsync<T>(string userId, int id, int taskId, ResponseDataMode mode)where T : class;

        Task UpdateNotes(string userId, Notes notes);

        Task AddNotes(Notes notes);

    }
}
