using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Models.Business
{
    public class DashboardService(IDashboardRepository repository) : IDashboardService
    {
        private readonly IDashboardRepository _repository = repository;

        public async Task<DashboardCountsDTO> GetDashboardAnalysesAsync(string userId)
        {
            return await _repository.GetDashboardAnalysesAsync(userId);
        }
    }
}
