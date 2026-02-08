using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace PayorClaims.Tests;

public static class JwtTestTokenFactory
{
    public static string BuildToken(IConfiguration configuration, string? role = null, Guid? sub = null, string? npi = null, Guid? memberId = null)
    {
        var key = configuration["Auth:SigningKey"] ?? "DEV_ONLY_32_CHARS_LONG_SECRET_KEY!!";
        var keyBytes = Encoding.UTF8.GetBytes(key.Length >= 32 ? key : key.PadRight(32));
        var issuer = configuration["Auth:Issuer"] ?? "payorclaims";
        var audience = configuration["Auth:Audience"] ?? "payorclaims";

        var claims = new List<Claim>();
        if (sub.HasValue) claims.Add(new Claim("sub", sub.Value.ToString()));
        if (!string.IsNullOrEmpty(role)) claims.Add(new Claim("role", role));
        if (!string.IsNullOrEmpty(npi)) claims.Add(new Claim("npi", npi));
        if (memberId.HasValue) claims.Add(new Claim("memberId", memberId.Value.ToString()));

        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
