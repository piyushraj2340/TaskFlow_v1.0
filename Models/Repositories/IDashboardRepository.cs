using TaskMonitoringApp.Models.DTOs;

namespace TaskMonitoringApp.Models.Repositories
{
    public interface IDashboardRepository
    {
        Task<DashboardCountsDTO> GetDashboardAnalysesAsync(string userId);
    }
}
