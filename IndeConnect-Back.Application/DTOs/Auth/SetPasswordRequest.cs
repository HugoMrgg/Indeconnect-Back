namespace IndeConnect_Back.Application.DTOs.Auth;

/// <summary>
/// DTO pour définir le mot de passe après avoir cliqué sur le lien d'invitation
/// </summary>
public record SetPasswordRequest(
    string Token,
    string Password,
    string ConfirmPassword
);