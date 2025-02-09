using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface INotesRepository
    {
        //Task<IEnumerable<T>> GetAllNotesAsync<T>(string userId, ResponseDataMode mode) where T: class;

        Task<IEnumerable<T>> GetAllNotesAsync<T>(string userId, Status status, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllNotesForGoalIdAsync<T>(string userId, int goalId, Status status, ResponseDataMode mode) where T : class;

        Task<IEnumerable<T>> GetAllNotesForTaskIdAsync<T>(string userId, int taskId, Status status, ResponseDataMode mode) where T : class;

        //Task<IEnumerable<T>> GetAllNotesForBothGoalAndTaskIdAsync<T>(string userId, int goalId, int taskId, Status status, ResponseDataMode mode) where T : class;

        Task<T> GetNotesByIdAsync<T>(string userId, int id, ResponseDataMode mode) where T : class;

        Task<T> GetNotesByIdWithGoalIdAsync<T>(string userId, int id, int goalId, ResponseDataMode mode)where T : class;

        Task<T> GetNotesByIdWithTaskIdAsync<T>(string userId, int id, int taskId, ResponseDataMode mode)where T : class;

        //Task<T> GetNotesByIdWithBothGoalAndTaskIdAsync<T>(string userId, int id, int goalId, int taskId, ResponseDataMode mode)where T : class;

        Task UpdateNotes(string userId, Notes notes);

        Task AddNotes(Notes notes);

    }
}
