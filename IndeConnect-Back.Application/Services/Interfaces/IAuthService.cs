using IndeConnect_Back.Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity.Data;
using RegisterRequest = IndeConnect_Back.Application.DTOs.Auth.RegisterRequest;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginAnonymousRequest request);
}