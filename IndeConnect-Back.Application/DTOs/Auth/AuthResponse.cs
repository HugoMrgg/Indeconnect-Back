namespace IndeConnect_Back.Application.DTOs.Auth;
/**
 * Represents an login's/register's request answer.
 */
public record AuthResponse(
    long UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Token
);