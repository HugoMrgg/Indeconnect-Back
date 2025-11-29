namespace IndeConnect_Back.Application.DTOs.Auth;

/// <summary>
/// DTO pour inviter un utilisateur (création de compte par un compte existant)
/// </summary>
public record InviteUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string TargetRole,
    long? CreatedBy = null
);