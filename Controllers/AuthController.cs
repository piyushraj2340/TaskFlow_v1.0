using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Services;

namespace TaskMonitoringApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, ILogger<AuthController> logger)
    {
        _auth = auth;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        _logger.LogInformation("Entered AuthController.Register email={Email} firstName={FirstName} lastName={LastName}", req.Email, req.FirstName, req.LastName);

        var (succeeded, errors) = await _auth.RegisterAsync(req);
        if (!succeeded)
        {
            _logger.LogWarning("Register failed for email={Email} errors={Errors}", req.Email, errors);
            return BadRequest(new { status = false, errors });
        }

        _logger.LogInformation("Register succeeded for email={Email}", req.Email);
        return Ok(new { status = true });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Entered AuthController.Login email={Email} ip={Ip}", req.Email, ip);

        var result = await _auth.LoginAsync(req, ip);
        if (!result.Success)
        {
            _logger.LogWarning("Login failed for email={Email} ip={Ip} errors={Errors}", req.Email, ip, result.Errors);
            return Unauthorized(new { status = false, errors = result.Errors });
        }

        _logger.LogInformation("Login succeeded for email={Email} ip={Ip}", req.Email, ip);
        return Ok(new { status = true, accessToken = result.AccessToken, refreshToken = result.RefreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var masked = string.IsNullOrEmpty(req.RefreshToken) ? "null" : req.RefreshToken[..Math.Min(8, req.RefreshToken.Length)] + "...";
        _logger.LogInformation("Entered AuthController.Refresh ip={Ip} refreshTokenStart={TokenStart}", ip, masked);

        var resp = await _auth.RefreshTokenAsync(req.RefreshToken, ip);
        if (resp == null)
        {
            _logger.LogWarning("Refresh token invalid or inactive ip={Ip} refreshTokenStart={TokenStart}", ip, masked);
            return Unauthorized(new { status = false, message = "Invalid refresh token." });
        }

        _logger.LogInformation("Refresh succeeded ip={Ip}", ip);
        return Ok(new { status = true, accessToken = resp.AccessToken, refreshToken = resp.RefreshToken });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var masked = string.IsNullOrEmpty(req.RefreshToken) ? "null" : req.RefreshToken[..Math.Min(8, req.RefreshToken.Length)] + "...";
        _logger.LogInformation("Entered AuthController.Logout ip={Ip} refreshTokenStart={TokenStart}", ip, masked);

        var revoked = await _auth.RevokeRefreshTokenAsync(req.RefreshToken, ip);
        if (!revoked)
        {
            _logger.LogWarning("Logout revoke failed (token not found or already revoked) ip={Ip} refreshTokenStart={TokenStart}", ip, masked);
            return BadRequest(new { status = false, message = "Token not found or already revoked." });
        }

        _logger.LogInformation("Logout succeeded ip={Ip}", ip);
        return Ok(new { status = true });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req, [FromQuery] string callbackBaseUrl)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Entered AuthController.ForgotPassword email={Email} ip={Ip} callbackBaseUrl={Callback}", req.Email, ip, callbackBaseUrl);

        var sent = await _auth.ForgotPasswordAsync(req, callbackBaseUrl ?? $"{Request.Scheme}://{Request.Host}");
        if (!sent)
        {
            _logger.LogWarning("ForgotPassword: email not found or not sent email={Email} ip={Ip}", req.Email, ip);
            return BadRequest(new { status = false });
        }

        _logger.LogInformation("ForgotPassword email sent email={Email} ip={Ip}", req.Email, ip);
        return Ok(new { status = true });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        _logger.LogInformation("Entered AuthController.ResetPassword email={Email}", req.Email);
        var (succeeded, errors) = await _auth.ResetPasswordAsync(req);
        if (!succeeded)
        {
            _logger.LogWarning("ResetPassword failed email={Email} errors={Errors}", req.Email, errors);
            return BadRequest(new { status = false, errors });
        }

        _logger.LogInformation("ResetPassword succeeded email={Email}", req.Email);
        return Ok(new { status = true });
    }

    [HttpGet("2fa/setup/{userId}")]
    public async Task<IActionResult> TwoFactorSetup(string userId, [FromQuery] string issuer = "TaskMonitoringApp")
    {
        _logger.LogInformation("Entered AuthController.TwoFactorSetup userId={UserId} issuer={Issuer}", userId, issuer);
        var resp = await _auth.GenerateTwoFactorSetupAsync(userId, issuer);
        return Ok(new { status = true, data = resp });
    }

    [HttpPost("2fa/enable/{userId}")]
    public async Task<IActionResult> Enable2Fa(string userId, [FromBody] string verificationCode)
    {
        _logger.LogInformation("Entered AuthController.Enable2Fa userId={UserId}", userId);
        var (succeeded, errors, recovery) = await _auth.EnableTwoFactorAsync(userId, verificationCode);
        if (!succeeded)
        {
            _logger.LogWarning("Enable2Fa failed userId={UserId} errors={Errors}", userId, errors);
            return BadRequest(new { status = false, errors });
        }

        _logger.LogInformation("Enable2Fa succeeded userId={UserId}", userId);
        return Ok(new { status = true, recoveryCodes = recovery });
    }

    [HttpPost("2fa/disable/{userId}")]
    public async Task<IActionResult> Disable2Fa(string userId)
    {
        _logger.LogInformation("Entered AuthController.Disable2Fa userId={UserId}", userId);
        var (succeeded, errors) = await _auth.DisableTwoFactorAsync(userId);
        if (!succeeded)
        {
            _logger.LogWarning("Disable2Fa failed userId={UserId} errors={Errors}", userId, errors);
            return BadRequest(new { status = false, errors });
        }

        _logger.LogInformation("Disable2Fa succeeded userId={UserId}", userId);
        return Ok(new { status = true });
    }
}