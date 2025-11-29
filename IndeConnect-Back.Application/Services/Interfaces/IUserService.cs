using IndeConnect_Back.Application.DTOs.Users;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IUserService
{
    Task<UserDetailDto?> GetUserByIdAsync(long userId);
    Task<List<AccountDto>> GetAllAccountsAsync();
    Task ToggleAccountStatusAsync(long accountId, bool isEnabled);
}