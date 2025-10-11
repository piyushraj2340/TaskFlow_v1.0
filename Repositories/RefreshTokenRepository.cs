using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _db;
    public RefreshTokenRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(RefreshToken token)
    {
        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();
    }

    public Task<RefreshToken?> GetByTokenAsync(string token)
        => _db.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.Token == token);

    public async Task UpdateAsync(RefreshToken token)
    {
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAsync(RefreshToken token, string revokedByIp, string? replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = revokedByIp;
        token.ReplacedByToken = replacedByToken;
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAllForUserAsync(string userId, string revokedByIp)
    {
        var tokens = await _db.RefreshTokens.Where(t => t.UserId == userId && t.IsActive).ToListAsync();
        foreach (var t in tokens)
        {
            t.Revoked = DateTime.UtcNow;
            t.RevokedByIp = revokedByIp;
        }
        _db.RefreshTokens.UpdateRange(tokens);
        await _db.SaveChangesAsync();
    }
}   