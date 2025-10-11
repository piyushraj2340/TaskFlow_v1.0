using System.Security.Claims;

namespace TaskMonitoringApp.Services;

public interface ITokenService
{
    string CreateToken(IEnumerable<Claim> claims, TimeSpan validFor);
    DateTime GetExpiry(DateTime now, TimeSpan validFor);
}