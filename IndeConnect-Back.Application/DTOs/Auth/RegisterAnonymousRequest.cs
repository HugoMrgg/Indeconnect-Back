namespace IndeConnect_Back.Application.DTOs.Auth;

public record RegisterAnonymousRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword
);