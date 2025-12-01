using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class PasswordResetTokenService : IPasswordResetTokenService
{
    private readonly AppDbContext _context;
    private readonly int _tokenExpirationHours = 24;

    public PasswordResetTokenService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreateResetTokenAsync(long userId)
    {
        // Invalide les anciens tokens pour cet utilisateur
        var oldTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow) 
            .ToListAsync();

        foreach (var oldToken in oldTokens)
        {
            oldToken.MarkAsUsed();
        }

        // Crée un nouveau token unique
        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddHours(_tokenExpirationHours);

        var resetToken = new PasswordResetToken(userId, token, expiresAt);
        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task<PasswordResetToken> ValidateAndUseTokenAsync(string token)
    {
        var resetToken = await GetValidTokenAsync(token);
        
        if (resetToken == null)
            throw new InvalidOperationException("Token invalid or expired");

        resetToken.MarkAsUsed();
        await _context.SaveChangesAsync();

        return resetToken;
    }

    public async Task<PasswordResetToken?> GetValidTokenAsync(string token)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => 
                t.Token == token && 
                !t.IsUsed && 
                t.ExpiresAt > DateTime.UtcNow); 
    }
}