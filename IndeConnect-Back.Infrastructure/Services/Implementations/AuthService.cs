using Google.Apis.Auth;
using IndeConnect_Back.Application.DTOs.Auth;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
        _frontendUrl = "http://localhost:5173"; /*Environment.GetEnvironmentVariable("FRONTEND_URL");*/
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
        await _context.SaveChangesAsync();
        
        await _auditTrail.LogAsync(
            action: "UserRegistered",
            userId: user.Id,
            details: $"{user.FirstName} {user.LastName} registered as {request.TargetRole}"
        );

        var token = _jwtGenerator.GenerateToken(user);

        return new AuthResponse(
            user.Id, 
            user.Email, 
            user.FirstName, 
            user.LastName, 
            user.Role, 
            token,
            null
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginAnonymousRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Brand)  // ✅ Include Brand au lieu de BrandsAsSuperVendor
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
        
        // ✅ Récupérer brandId directement depuis user.BrandId
        var brandId = user.Role == Role.SuperVendor 
            ? user.BrandId 
            : null;
        
        return new AuthResponse(
            user.Id, 
            user.Email, 
            user.FirstName, 
            user.LastName, 
            user.Role, 
            token,
            brandId
        );
    }

    public async Task InviteUserAsync(InviteUserRequest request, long invitedBy)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        // Cas : ré-invitation d'un utilisateur déjà existant (même email)
        if (existingUser != null)
        {
            if (existingUser.PasswordHash != null || !string.IsNullOrEmpty(existingUser.GoogleId))
            {
                throw new InvalidOperationException(
                    "Un compte existe déjà avec cet email."
                );
            }
            
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
                details: $"Existing user {existingUser.Email} re-invited by user {invitedBy}"
            );

            return;
        }

        var role = ParseRole(request.TargetRole);

        User user;
        Brand? brand = null;

        // Transaction atomique : User + Brand (si SuperVendor)
        await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                user = new User(
                    email: request.Email.Trim().ToLowerInvariant(),
                    firstName: request.FirstName.Trim(),
                    lastName: request.LastName.Trim(),
                    role: role
                );

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Nécessaire pour avoir user.Id

                // Si SuperVendor → Créer Brand vide
                if (role == Role.SuperVendor)
                {
                    brand = new Brand(
                        name: $"Brand_{user.Id}",  // Nom temporaire
                        superVendorUserId: user.Id
                    );

                    _context.Brands.Add(brand);
                    await _context.SaveChangesAsync(); // Brand.Id disponible

                    // ✅ Lier le User à sa Brand
                    user.SetBrand(brand.Id);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Envoi de l'email d'activation
        var token = await _resetTokenService.CreateResetTokenAsync(user.Id);
        var activationLinkNew = $"{_frontendUrl}/set-password?token={token}";
        var htmlContentNew = BuildActivationEmailHtml(user.FirstName, activationLinkNew);

        await _emailService.SendEmailAsync(
            user.Email,
            "Activez votre compte IndeConnect",
            htmlContentNew
        );

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
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException("Invalid Google token", ex);
        }

        var user = await _context.Users
            .Include(u => u.Brand)
            .FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (user == null)
        {
            string firstName;
            string lastName;

            if (!string.IsNullOrWhiteSpace(payload.GivenName) && 
                !string.IsNullOrWhiteSpace(payload.FamilyName))
            {
                firstName = payload.GivenName.Trim();
                lastName = payload.FamilyName.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(payload.Name))
            {
                var nameParts = payload.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                if (nameParts.Length >= 2)
                {
                    firstName = nameParts[0];
                    lastName = string.Join(" ", nameParts.Skip(1));
                }
                else
                {
                    // Un seul mot dans le nom (ex: "Madonna")
                    firstName = nameParts[0];
                    lastName = nameParts[0]; // On duplique
                }
            }
            else if (!string.IsNullOrWhiteSpace(payload.GivenName))
            {
                firstName = payload.GivenName.Trim();
                lastName = payload.GivenName.Trim(); // On duplique
            }
            else if (!string.IsNullOrWhiteSpace(payload.FamilyName))
            {
                firstName = payload.FamilyName.Trim();
                lastName = payload.FamilyName.Trim(); // On duplique
            }
            else
            {
                var emailName = payload.Email.Split('@')[0];
                firstName = emailName;
                lastName = emailName;
            }

            user = new User(
                email: payload.Email,
                firstName: firstName,
                lastName: lastName,
                role: Role.Client
            );

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

        var token = _jwtGenerator.GenerateToken(user);

        var brandId = user.Role == Role.SuperVendor 
            ? user.BrandId 
            : null;

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            token,
            brandId
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