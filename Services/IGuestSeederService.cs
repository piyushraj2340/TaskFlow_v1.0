using System.Threading.Tasks;

namespace TaskMonitoringApp.Services
{
    public interface IGuestSeederService
    {
        Task EnsureGuestDataExistsAsync();
        Task CleanupAndReseedGuestAsync();
    }
}
