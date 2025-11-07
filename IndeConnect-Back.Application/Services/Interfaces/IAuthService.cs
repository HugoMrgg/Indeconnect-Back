using IndeConnect_Back.Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity.Data;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAnonymousAsync(RegisterAnonymousRequest request);
    Task<AuthResponse> LoginAsync(LoginAnonymousRequest request);
}