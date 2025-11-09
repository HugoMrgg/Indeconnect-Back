using IndeConnect_Back.Application.DTOs.Users;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IUserService
{
    Task<UserDetailDto?> GetUserByIdAsync(long userId);
}