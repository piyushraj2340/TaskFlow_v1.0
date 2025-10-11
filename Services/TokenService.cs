using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TaskMonitoringApp.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config) => _config = config;

    public string CreateToken(IEnumerable<Claim> claims, TimeSpan validFor)
    {
        var secret = _config["Jwt:Secret"];
        var issuer = _config["Jwt:Issuer"] ?? "taskmonitoringapp";
        var audience = _config["Jwt:Audience"] ?? "taskmonitoringapp";

        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("JWT secret is not configured (Jwt:Secret).");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(validFor),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpiry(DateTime now, TimeSpan validFor) => now.Add(validFor);
}