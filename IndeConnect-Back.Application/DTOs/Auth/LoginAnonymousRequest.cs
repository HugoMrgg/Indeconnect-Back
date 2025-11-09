namespace IndeConnect_Back.Application.DTOs.Auth;
/**
 * Represents the body of a login's request
 */
public record LoginAnonymousRequest(
    string Email,
    string Password
);