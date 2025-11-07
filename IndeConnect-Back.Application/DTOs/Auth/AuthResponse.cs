namespace IndeConnect_Back.Application.DTOs.Auth;

public record AuthResponse(
    long UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Token
);