using System.Threading.Tasks;
using TaskMonitoringApp.Models.DTOs;

namespace TaskMonitoringApp.Services;

public interface IAuthService
{
    Task<(bool Succeeded, IEnumerable<string>? Errors)> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request, string callbackBaseUrl);
    Task<(bool Succeeded, IEnumerable<string>? Errors)> ResetPasswordAsync(ResetPasswordRequest request);
    Task<TwoFactorSetupResponse> GenerateTwoFactorSetupAsync(string userId, string issuer);
    Task<(bool Succeeded, IEnumerable<string>? Errors, IEnumerable<string>? RecoveryCodes)> EnableTwoFactorAsync(string userId, string verificationCode);
    Task<(bool Succeeded, IEnumerable<string>? Errors)> DisableTwoFactorAsync(string userId);
    Task<ProfileDto?> GetProfileAsync(string userId);
    Task<(bool Succeeded, IEnumerable<string>? Errors)> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<(bool Succeeded, IEnumerable<string>? Errors)> DeleteProfileAsync(string userId);
    Task<(bool Succeeded, IEnumerable<string>? Errors)> DeactivateAsync(string userId);

    // Refresh token methods
    Task<RefreshResponse?> RefreshTokenAsync(string token, string ipAddress);
    Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress);
    Task RevokeAllRefreshTokensForUserAsync(string userId, string ipAddress);
}