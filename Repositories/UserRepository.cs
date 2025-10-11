using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<Users> _userManager;

    public UserRepository(UserManager<Users> userManager)
    {
        _userManager = userManager;
    }

    public Task<Users?> FindByIdAsync(string id) => _userManager.FindByIdAsync(id);
    public Task<Users?> FindByEmailAsync(string email) => _userManager.FindByEmailAsync(email);
    public Task<IdentityResult> CreateAsync(Users user, string password) => _userManager.CreateAsync(user, password);
    public Task<IdentityResult> UpdateAsync(Users user) => _userManager.UpdateAsync(user);
    public async Task<IdentityResult> UpdateAsync(Users user, bool updateSecurityStamp)
    {
        if (updateSecurityStamp) await _userManager.UpdateSecurityStampAsync(user);
        return await _userManager.UpdateAsync(user);
    }
    public Task<IdentityResult> DeleteAsync(Users user) => _userManager.DeleteAsync(user);
    public Task<string> GeneratePasswordResetTokenAsync(Users user) => _userManager.GeneratePasswordResetTokenAsync(user);
    public Task<IdentityResult> ResetPasswordAsync(Users user, string token, string newPassword) => _userManager.ResetPasswordAsync(user, token, newPassword);
    public Task<bool> CheckPasswordAsync(Users user, string password) => _userManager.CheckPasswordAsync(user, password);
    public Task<string> GenerateEmailConfirmationTokenAsync(Users user) => _userManager.GenerateEmailConfirmationTokenAsync(user);
    public Task<IdentityResult> ConfirmEmailAsync(Users user, string token) => _userManager.ConfirmEmailAsync(user, token);
    public Task<string?> GetAuthenticatorKeyAsync(Users user) => _userManager.GetAuthenticatorKeyAsync(user);
    public Task ResetAuthenticatorKeyAsync(Users user) => _userManager.ResetAuthenticatorKeyAsync(user);
    public Task<IdentityResult> SetTwoFactorEnabledAsync(Users user, bool enabled) => _userManager.SetTwoFactorEnabledAsync(user, enabled);
    public async Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(Users user, int count)
    {
        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, count);
        return codes;
    }
}