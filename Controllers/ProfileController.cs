using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Services;

namespace TaskMonitoringApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "JwtBearer")]
public class ProfileController : ControllerBase
{
    private readonly IAuthService _auth;
    public ProfileController(IAuthService auth) => _auth = auth;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var profile = await _auth.GetProfileAsync(UserId);
        if (profile == null) return NotFound(new { status = false });
        return Ok(new { status = true, data = profile });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest req)
    {
        var (succeeded, errors) = await _auth.UpdateProfileAsync(UserId, req);
        if (!succeeded) return BadRequest(new { status = false, errors });
        return Ok(new { status = true });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var (succeeded, errors) = await _auth.DeleteProfileAsync(UserId);
        if (!succeeded) return BadRequest(new { status = false, errors });
        return Ok(new { status = true });
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> Deactivate()
    {
        var (succeeded, errors) = await _auth.DeactivateAsync(UserId);
        if (!succeeded) return BadRequest(new { status = false, errors });
        return Ok(new { status = true });
    }
}