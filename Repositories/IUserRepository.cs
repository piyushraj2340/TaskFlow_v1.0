using System.Threading.Tasks;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Repositories;

public interface IUserRepository
{
    Task<Users?> FindByIdAsync(string id);
    Task<Users?> FindByEmailAsync(string email);
    Task<Microsoft.AspNetCore.Identity.IdentityResult> CreateAsync(Users user, string password);
    Task<Microsoft.AspNetCore.Identity.IdentityResult> UpdateAsync(Users user);
    Task<Microsoft.AspNetCore.Identity.IdentityResult> DeleteAsync(Users user);
    Task<string> GeneratePasswordResetTokenAsync(Users user);
    Task<Microsoft.AspNetCore.Identity.IdentityResult> ResetPasswordAsync(Users user, string token, string newPassword);
    Task<bool> CheckPasswordAsync(Users user, string password);
    Task<string> GenerateEmailConfirmationTokenAsync(Users user);
    Task<Microsoft.AspNetCore.Identity.IdentityResult> ConfirmEmailAsync(Users user, string token);
    Task<string?> GetAuthenticatorKeyAsync(Users user);
    Task ResetAuthenticatorKeyAsync(Users user);
    Task<Microsoft.AspNetCore.Identity.IdentityResult> SetTwoFactorEnabledAsync(Users user, bool enabled);
    Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(Users user, int count);
    Task<Microsoft.AspNetCore.Identity.IdentityResult> UpdateAsync(Users user, bool updateSecurityStamp);
}