using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PayorClaims.Api.Options;

namespace PayorClaims.Api.Controllers;

/// <summary>
/// Authentication controller.
/// In production, replace dev users with a real identity provider.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticate with username and password. Returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Username and password are required." });

        var user = DevUsers.Find(request.Username, request.Password);
        if (user == null)
            return Unauthorized(new { error = "Invalid username or password." });

        var token = GenerateJwt(user);

        return Ok(new LoginResponse
        {
            Token = token.Jwt,
            ExpiresAt = token.ExpiresAt,
            User = new UserInfo
            {
                Username = user.Username,
                DisplayName = user.DisplayName,
                Roles = user.Roles,
            }
        });
    }

    private (string Jwt, DateTime ExpiresAt) GenerateJwt(DevUser user)
    {
        var authOptions = _configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
        var key = authOptions.SigningKey;
        if (string.IsNullOrEmpty(key))
            key = "DEV_ONLY_CHANGE_ME_32+_CHARS_LONG_SECRET";

        var keyBytes = Encoding.UTF8.GetBytes(key.Length >= 32 ? key : key.PadRight(32));

        var claims = new List<Claim>
        {
            new("sub", user.Sub),
            new("name", user.DisplayName),
        };

        foreach (var role in user.Roles)
            claims.Add(new Claim("role", role));

        if (!string.IsNullOrEmpty(user.Npi))
            claims.Add(new Claim("npi", user.Npi));

        if (!string.IsNullOrEmpty(user.MemberId))
            claims.Add(new Claim("memberId", user.MemberId));

        var expiresAt = DateTime.UtcNow.AddHours(8);
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: authOptions.Issuer,
            audience: authOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

// ── Request / Response models ──────────────────────────────────────

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string[] Roles { get; set; } = [];
}

// ── Dev users (replace with DB / identity provider in production) ──

public record DevUser(
    string Username,
    string Password,
    string DisplayName,
    string Sub,
    string[] Roles,
    string? Npi = null,
    string? MemberId = null);

public static class DevUsers
{
    private static readonly DevUser[] Users =
    [
        new("admin",    "admin123",    "Sarah Admin",       "admin-001",    ["Admin"]),
        new("adjuster", "adjuster123", "James Adjuster",    "adjuster-001", ["Adjuster"]),
        new("provider", "provider123", "Dr. Smith",         "provider-001", ["Provider"],  Npi: "1234567890"),
        new("member",   "member123",   "John Member",       "member-001",   ["Member"],    MemberId: "00000000-0000-0000-0000-000000000001"),
        new("super",    "super123",    "Super User",        "super-001",    ["Admin", "Adjuster", "Provider", "Member"]),
    ];

    public static DevUser? Find(string username, string password)
        => Users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);
}
