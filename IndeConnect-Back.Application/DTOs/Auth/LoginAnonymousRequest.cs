namespace IndeConnect_Back.Application.DTOs.Auth;

public record LoginAnonymousRequest(
    string Email,
    string Password
);