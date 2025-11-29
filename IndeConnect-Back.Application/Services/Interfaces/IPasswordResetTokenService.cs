using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IPasswordResetTokenService
{
    /// <summary>
    /// Crée un token d'activation/réinitialisation de mot de passe
    /// </summary>
    Task<string> CreateResetTokenAsync(long userId);

    /// <summary>
    /// Valide et utilise un token (le marque comme utilisé)
    /// </summary>
    Task<PasswordResetToken> ValidateAndUseTokenAsync(string token);

    /// <summary>
    /// Récupère un token non expiré et non utilisé
    /// </summary>
    Task<PasswordResetToken?> GetValidTokenAsync(string token);
}