using System.Threading.Tasks;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task UpdateAsync(RefreshToken token);
    Task RevokeAsync(RefreshToken token, string revokedByIp, string? replacedByToken = null);
    Task RevokeAllForUserAsync(string userId, string revokedByIp);
}