using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenGenerator()
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET"); 

        _secret  = jwtSecret
                   ?? throw new InvalidOperationException("JWT_SECRET not configured");
        _issuer  = Environment.GetEnvironmentVariable("JWT_ISSUER")   
                   ?? throw new InvalidOperationException("JWT_ISSUER not configured");
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")  
                    ?? throw new InvalidOperationException("JWT_AUDIENCE not configured");

        if (_secret.Length < 32)
            throw new InvalidOperationException("JWT_SECRET must be at least 32 characters long.");
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}