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
        // Récupérer l'utilisateur qui invite (pour vérifier son rôle et BrandId)
        var invitingUser = await _context.Users
            .Include(u => u.Brand)
            .FirstOrDefaultAsync(u => u.Id == invitedBy);

        if (invitingUser == null)
            throw new InvalidOperationException("Inviting user not found.");

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

            // Génère un nouveau token d'invitation
            var newToken = Guid.NewGuid().ToString("N");
            var expiresAt = DateTimeOffset.UtcNow.AddHours(24);
            existingUser.SetInvitationToken(newToken, expiresAt);
            await _context.SaveChangesAsync();

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

                user.SetEnabled(true);

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

                // Si un SuperVendor invite un Vendor → Créer BrandSeller pour l'associer à la marque
                if (role == Role.Vendor && invitingUser.Role == Role.SuperVendor)
                {
                    if (invitingUser.BrandId == null)
                        throw new InvalidOperationException("SuperVendor must have a Brand to invite Vendors.");

                    var brandSeller = new BrandSeller(invitingUser.BrandId.Value, user.Id);
                    _context.BrandSellers.Add(brandSeller);
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

        // Génère le token d'invitation
        var token = Guid.NewGuid().ToString("N");
        var tokenExpiresAt = DateTimeOffset.UtcNow.AddHours(24);
        user.SetInvitationToken(token, tokenExpiresAt);
        await _context.SaveChangesAsync();

        // Envoi de l'email d'activation
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

        // Chercher d'abord un utilisateur avec ce token d'invitation
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.InvitationTokenHash == request.Token);

        if (user != null)
        {
            // Cas : Activation via token d'invitation
            if (user.InvitationExpiresAt == null || user.InvitationExpiresAt < DateTimeOffset.UtcNow)
                throw new InvalidOperationException("Invitation token expired.");

            // Définit le mot de passe
            user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(request.Password));

            // Efface le token d'invitation
            user.ClearInvitationToken();

            // Activer le compte
            user.SetEnabled(true);

            await _auditTrail.LogAsync(
                action: "UserActivated",
                userId: user.Id,
                details: $"{user.FirstName} {user.LastName} activated their account"
            );

            await _context.SaveChangesAsync();
            return;
        }

        // Sinon, vérifier si c'est un token de reset de mot de passe
        var resetToken = await _resetTokenService.ValidateAndUseTokenAsync(request.Token);

        // Récupère l'utilisateur
        user = await _context.Users.FindAsync(resetToken.UserId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        // Définit le mot de passe
        user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(request.Password));

        // Activer le compte maintenant que le mot de passe est défini
        user.SetEnabled(true);

        await _auditTrail.LogAsync(
            action: "PasswordReset",
            userId: user.Id,
            details: $"{user.FirstName} {user.LastName} reset their password"
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
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse; background-color: #f4f4f4;"">
        <tr>
            <td align=""center"" style=""padding: 20px 0;"">
                <table role=""presentation"" style=""max-width: 600px; width: 100%; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; overflow: hidden;"">
                    
                    <!-- Header -->
                    <tr>
                        <td style=""background-color: #000000; padding: 30px; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-family: Arial, sans-serif;"">IndeConnect</h1>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px 30px; background-color: #f9f9f9;"">
                            <p style=""margin: 0 0 20px 0; font-size: 16px; line-height: 24px; color: #333333; font-family: Arial, sans-serif;"">
                                Bonjour <strong>{firstName}</strong>,
                            </p>
                            <p style=""margin: 0 0 20px 0; font-size: 16px; line-height: 24px; color: #333333; font-family: Arial, sans-serif;"">
                                Un compte a été créé pour vous sur IndeConnect.
                            </p>
                            <p style=""margin: 0 0 30px 0; font-size: 16px; line-height: 24px; color: #333333; font-family: Arial, sans-serif;"">
                                Cliquez sur le bouton ci-dessous pour activer votre compte et définir votre mot de passe :
                            </p>
                            
                            <!-- Button (structure table pour compatibilité) -->
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
                                <tr>
                                    <td align=""center"" style=""padding: 10px 0;"">
                                        <table role=""presentation"" style=""border-collapse: collapse;"">
                                            <tr>
                                                <td style=""background-color: #000000; border-radius: 5px; text-align: center;"">
                                                    <a href=""{activationLink}"" 
                                                       style=""display: inline-block; 
                                                              padding: 16px 32px; 
                                                              background-color: #000000; 
                                                              color: #ffffff; 
                                                              text-decoration: none; 
                                                              border-radius: 5px; 
                                                              font-size: 16px; 
                                                              font-weight: bold;
                                                              font-family: Arial, sans-serif;"">
                                                        Activer mon compte
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style=""margin: 30px 0 0 0; font-size: 12px; line-height: 18px; color: #666666; font-family: Arial, sans-serif;"">
                                Ce lien expire dans 24 heures.
                            </p>
                            
                            <!-- Lien de secours -->
                            <p style=""margin: 20px 0 0 0; font-size: 11px; line-height: 16px; color: #999999; font-family: Arial, sans-serif;"">
                                Si le bouton ne fonctionne pas, copiez-collez ce lien dans votre navigateur :
                            </p>
                            <p style=""margin: 5px 0 0 0; font-size: 11px; line-height: 16px; word-break: break-all; font-family: Arial, sans-serif;"">
                                <a href=""{activationLink}"" style=""color: #0066cc; text-decoration: underline;"">{activationLink}</a>
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 30px; text-align: center; background-color: #ffffff; border-top: 1px solid #eeeeee;"">
                            <p style=""margin: 0; font-size: 12px; color: #666666; font-family: Arial, sans-serif;"">
                                &copy; 2025 IndeConnect. Tous droits réservés.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
}
}