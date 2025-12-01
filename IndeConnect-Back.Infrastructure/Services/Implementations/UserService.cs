using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserDetailDto?> GetUserByIdAsync(long userId)
    {
        var user = await _context.Users
            .Include(u => u.BrandSubscriptions)
            .Include(u => u.Reviews)
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        return new UserDetailDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            user.IsEnabled,
            user.Role,           
            user.BrandSubscriptions.Count,
            user.Reviews.Count,
            user.Orders.Count
        );
    }

    public async Task<List<AccountDto>> GetAllAccountsAsync()
    {
        var adminRoles = new[] { Role.Administrator, Role.Moderator, Role.SuperVendor };

        var accounts = await _context.Users
            .Where(u => adminRoles.Contains(u.Role))
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AccountDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,                   
                u.IsEnabled,
                u.PasswordHash == null     
            ))
            .ToListAsync();

        return accounts;
    }

    public async Task ToggleAccountStatusAsync(long accountId, bool isEnabled)
    {
        var account = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == accountId);

        if (account == null)
            throw new KeyNotFoundException($"Account with ID {accountId} not found");

        account.SetEnabled(isEnabled);
        await _context.SaveChangesAsync();
    }
}