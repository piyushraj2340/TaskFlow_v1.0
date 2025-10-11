using System.Collections.Generic;

namespace TaskMonitoringApp.Models.DTOs;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public record LoginRequest(string Email, string Password, bool RememberMe = false);
public record LoginResponse(bool Success, string? AccessToken = null, string? RefreshToken = null, IEnumerable<string>? Errors = null);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record TwoFactorLoginRequest(string Email, string TwoFactorCode, bool RememberClient = false, bool RememberMe = false);
public record TwoFactorSetupResponse(string SharedKey, string AuthenticatorUri);
public record ProfileDto(string Id, string Email, string FirstName, string LastName, bool EmailConfirmed);
public record UpdateProfileRequest(string FirstName, string LastName, string? PhoneNumber);
public record RefreshRequest(string RefreshToken);
public record RefreshResponse(string AccessToken, string RefreshToken); 