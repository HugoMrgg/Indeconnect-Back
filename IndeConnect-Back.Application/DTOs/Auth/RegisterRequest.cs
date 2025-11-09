namespace IndeConnect_Back.Application.DTOs.Auth;
/**
 * Represents the body of a register's request
 */
public record RegisterRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword,
    string TargetRole
);