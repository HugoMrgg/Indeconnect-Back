using IndeConnect_Back.Application.DTOs.Auth;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginAnonymousRequest request);
    Task InviteUserAsync(InviteUserRequest request, long invitedBy);
    Task SetPasswordAsync(SetPasswordRequest request);
    Task<AuthResponse> GoogleAuthAsync(GoogleAuthRequest request);
}