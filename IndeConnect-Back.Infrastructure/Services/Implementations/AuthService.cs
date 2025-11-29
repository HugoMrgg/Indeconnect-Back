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
    private readonly IEmailService _emailService;
    private readonly IPasswordResetTokenService _resetTokenService;

    public AuthService(
        AppDbContext context,
        IJwtTokenGenerator jwtGenerator,
        IAuditTrailService auditTrail,
        IEmailService emailService,
        IPasswordResetTokenService resetTokenService)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
        _auditTrail = auditTrail;
        _emailService = emailService;
        _resetTokenService = resetTokenService;
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
            details: $"{user.FirstName} {user.LastName} registered as {request.TargetRole}"
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
            details: $"{user.FirstName} {user.LastName} logged in"
        );
        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString(), token);
    }

    public async Task InviteUserAsync(InviteUserRequest request, long invitedBy)
    {
        // Vérifie que l'email n'existe pas déjà
        var existing = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (existing)
            throw new InvalidOperationException("Email already registered.");

        // Crée le nouvel utilisateur SANS mot de passe
        var user = new User(
            email: request.Email,
            firstName: request.FirstName,
            lastName: request.LastName,
            role: ParseRole(request.TargetRole)
        );

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Génère un token d'activation
        var token = await _resetTokenService.CreateResetTokenAsync(user.Id);

        // Envoie l'email avec le token (et le lien est généré dans SendGridEmailService)
        await _emailService.SendActivationEmailAsync(user.Email, user.FirstName, token);

        // Audit
        await _auditTrail.LogAsync(
            action: "UserInvited",
            userId: invitedBy,
            details: $"User {user.Email} ({request.TargetRole}) invited by user {invitedBy}"
        );
    }

    public async Task SetPasswordAsync(SetPasswordRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            throw new InvalidOperationException("Passwords do not match.");

        // Valide et utilise le token
        var resetToken = await _resetTokenService.ValidateAndUseTokenAsync(request.Token);

        // Récupère l'utilisateur
        var user = await _context.Users.FindAsync(resetToken.UserId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        // Définit le mot de passe
        user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(request.Password));

        await _auditTrail.LogAsync(
            action: "UserPasswordSet",
            userId: user.Id,
            details: $"{user.FirstName} {user.LastName} set their password"
        );

        await _context.SaveChangesAsync();
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
