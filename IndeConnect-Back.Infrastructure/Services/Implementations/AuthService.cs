using IndeConnect_Back.Application.DTOs.Auth;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AuthService(AppDbContext context, IJwtTokenGenerator jwtGenerator)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email already exists");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var targetRole = ParseRole(request.TargetRole);

        var user = new User(
            request.Email,
            request.FirstName,
            request.LastName,
            targetRole
            );

        user.SetPasswordHash(passwordHash);
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtGenerator.GenerateToken(user);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            token
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginAnonymousRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        // Verify password with user.PasswordHash
        if (string.IsNullOrEmpty(user.PasswordHash) || 
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var token = _jwtGenerator.GenerateToken(user);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            token
        );
    }

    private Role ParseRole(string role)
    {
        return role switch
        {
            "client" => Role.Client,
            "vendor" => Role.Vendor,
            "supervendor" => Role.SuperVendor,
            "moderator" => Role.Moderator,
            "administrator" => Role.Administrator
        };

    }
}
