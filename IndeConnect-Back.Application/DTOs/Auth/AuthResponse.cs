using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Application.DTOs.Auth;
/**
 * Represents an login's/register's request answer.
 */
public record AuthResponse(
    long UserId,
    string Email,
    string FirstName,
    string LastName,
    Role Role,
    string Token,
    long? BrandId
);