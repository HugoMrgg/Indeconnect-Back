using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Application.DTOs.Users;

/// <summary>
/// Représentation d'un compte administratif
/// </summary>
public record AccountDto(
    long Id,
    string Email,
    string FirstName,
    string LastName,
    Role Role,
    bool IsEnabled,
    bool IsPendingActivation
);