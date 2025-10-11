using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Repositories;

namespace TaskMonitoringApp.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly SignInManager<Users> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;
    private readonly UrlEncoder _urlEncoder = UrlEncoder.Default;
    private readonly TimeSpan _accessTokenValidity = TimeSpan.FromHours(8);
    private readonly TimeSpan _refreshTokenValidity = TimeSpan.FromDays(30);

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshRepo,
        SignInManager<Users> signInManager,
        ITokenService tokenService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _users = users;
        _refreshRepo = refreshRepo;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<(bool Succeeded, IEnumerable<string>? Errors)> RegisterAsync(RegisterRequest request)
    {
        var existing = await _users.FindByEmailAsync(request.Email);
        if (existing != null) return (false, new[] { "Email already registered." });

        var user = new Users
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = false
        };

        var result = await _users.CreateAsync(user, request.Password);
        return (result.Succeeded, result.Succeeded ? null : result.Errors.Select(e => e.Description));
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        var user = await _users.FindByEmailAsync(request.Email);
        if (user == null) return new LoginResponse(false, null, null, new[] { "Invalid credentials." });

        var check = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!check.Succeeded) return new LoginResponse(false, null, null, new[] { "Invalid credentials." });

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };

        var accessToken = _tokenService.CreateToken(claims, _accessTokenValidity);

        // create refresh token
        var refresh = CreateRefreshToken(user.Id, ipAddress, _refreshTokenValidity);
        await _refreshRepo.AddAsync(refresh);

        return new LoginResponse(true, accessToken, refresh.Token, null);
    }

    public async Task<RefreshResponse?> RefreshTokenAsync(string token, string ipAddress)
    {
        var existing = await _refreshRepo.GetByTokenAsync(token);
        if (existing == null || !existing.IsActive) return null;

        // rotate: revoke existing and create a new one
        var newRefresh = CreateRefreshToken(existing.UserId, ipAddress, _refreshTokenValidity);
        existing.Revoked = DateTime.UtcNow;
        existing.RevokedByIp = ipAddress;
        existing.ReplacedByToken = newRefresh.Token;
        await _refreshRepo.UpdateAsync(existing);
        await _refreshRepo.AddAsync(newRefresh);

        // create new access token
        var user = existing.User!;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };
        var accessToken = _tokenService.CreateToken(claims, _accessTokenValidity);

        return new RefreshResponse(accessToken, newRefresh.Token);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress)
    {
        var existing = await _refreshRepo.GetByTokenAsync(token);
        if (existing == null || !existing.IsActive) return false;
        await _refreshRepo.RevokeAsync(existing, ipAddress, null);
        return true;
    }

    public async Task RevokeAllRefreshTokensForUserAsync(string userId, string ipAddress)
    {
        await _refreshRepo.RevokeAllForUserAsync(userId, ipAddress);
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request, string callbackBaseUrl)
    {
        var user = await _users.FindByEmailAsync(request.Email);
        if (user == null) return false;

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var encoded = _urlEncoder.Encode(token);
        var link = $"{callbackBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={encoded}";

        await _emailService.SendAsync(user.Email!, "Reset your password", $"Click here to reset your password: <a href=\"{link}\">{link}</a>");
        return true;
    }

    public async Task<(bool Succeeded, IEnumerable<string>? Errors)> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email);
        if (user == null) return (false, new[] { "User not found." });

        var result = await _users.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return (result.Succeeded, result.Succeeded ? null : result.Errors.Select(e => e.Description));
    }

    public async Task<TwoFactorSetupResponse> GenerateTwoFactorSetupAsync(string userId, string issuer)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) throw new InvalidOperationException("User not found.");

        var key = await _users.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _users.ResetAuthenticatorKeyAsync(user);
            key = await _users.GetAuthenticatorKeyAsync(user) ?? "";
        }

        var encodedKey = key;
        var email = user.Email ?? user.UserName;
        var authenticatorUri = $"otpauth://totp/{UrlEncoder.Default.Encode(issuer)}:{UrlEncoder.Default.Encode(email ?? "")}?secret={encodedKey}&issuer={UrlEncoder.Default.Encode(issuer)}&digits=6";
        return new TwoFactorSetupResponse(encodedKey, authenticatorUri);
    }

    public async Task<(bool Succeeded, IEnumerable<string>? Errors, IEnumerable<string>? RecoveryCodes)> EnableTwoFactorAsync(string userId, string verificationCode)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return (false, new[] { "User not found." }, null);

        var isValid = await _signInManager.UserManager.VerifyTwoFactorTokenAsync(user, _signInManager.UserManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);
        if (!isValid) return (false, new[] { "Invalid verification code." }, null);

        var result = await _users.SetTwoFactorEnabledAsync(user, true);
        if (!result.Succeeded) return (false, result.Errors.Select(e => e.Description), null);

        var recovery = await _users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return (true, null, recovery);
    }

    public async Task<(bool Succeeded, IEnumerable<string>? Errors)> DisableTwoFactorAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return (false, new[] { "User not found." });

        var result = await _users.SetTwoFactorEnabledAsync(user, false);
        return (result.Succeeded, result.Succeeded ? null : result.Errors.Select(e => e.Description));
    }

    public async Task<ProfileDto?> GetProfileAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return null;
        return new ProfileDto(user.Id, user.Email ?? "", user.FirstName ?? "", user.LastName ?? "", user.EmailConfirmed);
    }

    public async Task<(bool Succeeded, IEnumerable<string>? Errors)> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return (false, new[] { "User not found." });

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        var result = await _users.UpdateAsync(user);
        return (result.Succeeded, result.Succeeded ? null : result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string>? Errors)> DeleteProfileAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return (false, new[] { "User not found." });

        var result = await _users.DeleteAsync(user);
        return (result.Succeeded, result.Succeeded ? null : result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string>? Errors)> DeactivateAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return (false, new[] { "User not found." });

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        var result = await _users.UpdateAsync(user);
        return (result.Succeeded, result.Succeeded ? null : result.Errors.Select(e => e.Description));
    }

    private static RefreshToken CreateRefreshToken(string userId, string ipAddress, TimeSpan lifetime)
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        var token = Convert.ToBase64String(randomBytes);
        return new RefreshToken
        {
            Token = token,
            UserId = userId,
            Expires = DateTime.UtcNow.Add(lifetime),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }
}