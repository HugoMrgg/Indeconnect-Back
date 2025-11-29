namespace IndeConnect_Back.Application.DTOs.Users;

/// <summary>
/// Représentation d'un compte administratif
/// </summary>
public record AccountDto(
    long Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsEnabled
);