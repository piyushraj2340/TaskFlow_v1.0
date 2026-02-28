using TaskMonitoringApp.Models.DTOs;

namespace TaskMonitoringApp.Models.Services
{
    public interface IDashboardService
    {
        Task<DashboardCountsDTO> GetDashboardAnalysesAsync(string userId);
    }
}
