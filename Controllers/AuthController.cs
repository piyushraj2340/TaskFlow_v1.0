using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Services;

namespace TaskMonitoringApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var (succeeded, errors) = await _auth.RegisterAsync(req);
        if (!succeeded) return BadRequest(new { status = false, errors });
        return Ok(new { status = true });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _auth.LoginAsync(req, ip);
        if (!result.Success) return Unauthorized(new { status = false, errors = result.Errors });
        return Ok(new { status = true, accessToken = result.AccessToken, refreshToken = result.RefreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var resp = await _auth.RefreshTokenAsync(req.RefreshToken, ip);
        if (resp == null) return Unauthorized(new { status = false, message = "Invalid refresh token." });
        return Ok(new { status = true, accessToken = resp.AccessToken, refreshToken = resp.RefreshToken });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var revoked = await _auth.RevokeRefreshTokenAsync(req.RefreshToken, ip);
        if (!revoked) return BadRequest(new { status = false, message = "Token not found or already revoked." });
        return Ok(new { status = true });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req, [FromQuery] string callbackBaseUrl)
    {
        var sent = await _auth.ForgotPasswordAsync(req, callbackBaseUrl ?? $"{Request.Scheme}://{Request.Host}");
        if (!sent) return BadRequest(new { status = false });
        return Ok(new { status = true });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var (succeeded, errors) = await _auth.ResetPasswordAsync(req);
        if (!succeeded) return BadRequest(new { status = false, errors });
        return Ok(new { status = true });
    }

    [HttpGet("2fa/setup/{userId}")]
    public async Task<IActionResult> TwoFactorSetup(string userId, [FromQuery] string issuer = "TaskMonitoringApp")
    {
        var resp = await _auth.GenerateTwoFactorSetupAsync(userId, issuer);
        return Ok(new { status = true, data = resp });
    }

    [HttpPost("2fa/enable/{userId}")]
    public async Task<IActionResult> Enable2Fa(string userId, [FromBody] string verificationCode)
    {
        var (succeeded, errors, recovery) = await _auth.EnableTwoFactorAsync(userId, verificationCode);
        if (!succeeded) return BadRequest(new { status = false, errors });
        return Ok(new { status = true, recoveryCodes = recovery });
    }

    [HttpPost("2fa/disable/{userId}")]
    public async Task<IActionResult> Disable2Fa(string userId)
    {
        var (succeeded, errors) = await _auth.DisableTwoFactorAsync(userId);
        if (!succeeded) return BadRequest(new { status = false, errors });
        return Ok(new { status = true });
    }
}