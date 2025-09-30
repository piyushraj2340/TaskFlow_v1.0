using TaskMonitoringApp.Models.DTOs;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface ISearchRepository
    {
        /// <summary>
        /// Search across goals, tasks, todos and notes for a user. Returns paged unified results ordered by timestamp desc.
        /// </summary>
        Task<IEnumerable<SearchResultDTO>> SearchAsync(string userId, string query, int pageNumber, int pageSize);
    }
}