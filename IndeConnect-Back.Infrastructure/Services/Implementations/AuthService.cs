using IndeConnect_Back.Application.DTOs.Auth;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IAuditTrailService _auditTrail;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AuthService(AppDbContext context, IJwtTokenGenerator jwtGenerator, IAuditTrailService auditTrail)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
        _auditTrail = auditTrail;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (existing)
            throw new InvalidOperationException("Email already registered.");

        var user = new User(
            email: request.Email,
            firstName: request.FirstName,
            lastName: request.LastName,
            role: ParseRole(request.TargetRole)
        );

        user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(request.Password));

        _context.Users.Add(user);
        await _auditTrail.LogAsync(
            action: "UserRegistered",
            userId: user.Id,
            details: $"{user.FirstName} {user.LastName} had been registered"
        );
        await _context.SaveChangesAsync();

        var token = _jwtGenerator.GenerateToken(user);

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString(), token);
    }

    public async Task<AuthResponse> LoginAsync(LoginAnonymousRequest request)
    {
        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !user.VerifyPassword(request.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _jwtGenerator.GenerateToken(user);
        await _auditTrail.LogAsync(
            action: "UserLogged",
            userId: user.Id,
            details: $"{user.FirstName} {user.LastName} had been logged in"
        );
        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString(), token);
    }

    private Role ParseRole(string role)
    {
        return role.ToLowerInvariant() switch
        {
            "client" => Role.Client,
            "vendor" => Role.Vendor,
            "supervendor" => Role.SuperVendor,
            "moderator" => Role.Moderator,
            "administrator" => Role.Administrator,
            _ => throw new ArgumentOutOfRangeException(nameof(role), "Unknown role")
        };
    }
}
