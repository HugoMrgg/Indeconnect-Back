using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IUserService
{
    Task<UserDetailDto?> GetUserByIdAsync(long? userId);
    Task<List<AccountDto>> GetAllAccountsAsync(long? currentUserId, Role currentUserRole);
    Task ToggleAccountStatusAsync(long accountId, bool isEnabled);
}