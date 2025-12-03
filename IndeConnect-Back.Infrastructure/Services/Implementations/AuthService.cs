using Google.Apis.Auth;
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
    private readonly string _frontendUrl;
    private readonly string? _googleClientId;
    
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
        _googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");

        _frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
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

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, user.Role, token);
    }

    public async Task<AuthResponse> LoginAsync(LoginAnonymousRequest request)
    {
        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid credentials.");
        
        if (user.PasswordHash == null)
            throw new UnauthorizedAccessException("Account not activated. Please check your email.");

        if (user.PasswordHash == null && user.GoogleId != null)
        {
            throw new UnauthorizedAccessException(
                "Ce compte utilise Google. Cliquez sur 'Se connecter avec Google'."
            );
        }

        if (!user.IsEnabled)
            throw new UnauthorizedAccessException("Account is disabled.");

        if (!user.VerifyPassword(request.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _jwtGenerator.GenerateToken(user);
        await _auditTrail.LogAsync(
            action: "UserLogged",
            userId: user.Id,
            details: $"{user.FirstName} {user.LastName} logged in"
        );
        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, user.Role, token);
    }

    public async Task InviteUserAsync(InviteUserRequest request, long invitedBy)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            if (existingUser.PasswordHash != null)
                throw new InvalidOperationException("Email already registered and active.");

            var newToken = await _resetTokenService.CreateResetTokenAsync(existingUser.Id);
            var activationLink = $"{_frontendUrl}/set-password?token={newToken}";
            var htmlContent = BuildActivationEmailHtml(existingUser.FirstName, activationLink);

            await _emailService.SendEmailAsync(
                existingUser.Email,
                "Rappel : Activez votre compte IndeConnect",
                htmlContent
            );

            await _auditTrail.LogAsync(
                action: "UserReinvited",
                userId: invitedBy,
                details: $"User {existingUser.Email} reinvited by user {invitedBy}"
            );

            return;
        }

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
        
        // Génère le lien d'activation
        var activationLinkNew = $"{_frontendUrl}/set-password?token={token}";
        
        // Génère le contenu HTML
        var htmlContentNew = BuildActivationEmailHtml(user.FirstName, activationLinkNew);
        
        // Envoie l'email
        await _emailService.SendEmailAsync(
            user.Email,
            "Activez votre compte IndeConnect",
            htmlContentNew
        );

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
            action: "UserActivated",
            userId: user.Id,
            details: $"{user.FirstName} {user.LastName} activated their account"
        );

        await _context.SaveChangesAsync();
    }

     public async Task<AuthResponse> GoogleAuthAsync(GoogleAuthRequest request)
    {   
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleClientId }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException("Invalid Google token", ex);
        }

        // 2. Chercher l'utilisateur par email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == payload.Email);

        // 3. Si n'existe pas → Créer
        if (user == null)
        {
            user = new User(
                email: payload.Email,
                firstName: payload.GivenName ?? "User",
                lastName: payload.FamilyName ?? "",
                role: Role.Client
            );

            // Pas de mot de passe pour Google
            user.GoogleId = payload.Subject;
            user.SetEnabled(true);

            _context.Users.Add(user);
            
            await _auditTrail.LogAsync(
                action: "UserRegisteredViaGoogle",
                userId: user.Id,
                details: $"{user.FirstName} {user.LastName} registered via Google"
            );
            
            await _context.SaveChangesAsync();
        }
        else
        {
            // Vérifier que le compte n'utilise pas déjà email/password
            if (user.PasswordHash != null && user.GoogleId == null)
            {
                throw new InvalidOperationException(
                    "Un compte existe déjà avec cet email. Connectez-vous avec votre mot de passe."
                );
            }
            
            await _auditTrail.LogAsync(
                action: "UserLoggedViaGoogle",
                userId: user.Id,
                details: $"{user.FirstName} {user.LastName} logged in via Google"
            );
        }

        // 4. Générer JWT
        var token = _jwtGenerator.GenerateToken(user);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            token
        );
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

    private static string BuildActivationEmailHtml(string firstName, string activationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #000; color: #fff; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ 
            display: inline-block; 
            background-color: #000; 
            color: #fff; 
            padding: 12px 24px; 
            text-decoration: none; 
            border-radius: 5px; 
            margin-top: 20px; 
        }}
        .footer {{ text-align: center; padding-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>IndeConnect</h1>
        </div>
        <div class=""content"">
            <p>Bonjour {firstName},</p>
            <p>Un compte a été créé pour vous sur IndeConnect.</p>
            <p>Cliquez sur le lien ci-dessous pour activer votre compte et définir votre mot de passe :</p>
            <a href=""{activationLink}"" class=""button"">Activer mon compte</a>
            <p style=""margin-top: 20px; color: #666; font-size: 12px;"">
                Ce lien expire dans 24 heures.
            </p>
        </div>
        <div class=""footer"">
            <p>&copy; 2025 IndeConnect. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";
    }
}
