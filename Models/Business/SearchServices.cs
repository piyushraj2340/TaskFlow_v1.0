using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Models.Business
{
    public class SearchServices(ISearchRepository repository) : ISearchServices
    {
        private readonly ISearchRepository _repository = repository;

        public async Task<IEnumerable<SearchResultDTO>> Search(string userId, string query, int pageNumber = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(userId)) return Enumerable.Empty<SearchResultDTO>();
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<SearchResultDTO>();

            return await _repository.SearchAsync(userId, query, pageNumber, pageSize);
        }
    }
}