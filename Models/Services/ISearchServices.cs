using TaskMonitoringApp.Models.DTOs;

namespace TaskMonitoringApp.Models.Services
{
    public interface ISearchServices
    {
        Task<IEnumerable<SearchResultDTO>> Search(string userId, string query, int pageNumber = 1, int pageSize = 20);
    }
}