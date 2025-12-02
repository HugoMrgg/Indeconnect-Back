using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;
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

    public async Task<List<AccountDto>> GetAllAccountsAsync(long currentUserId, Role currentUserRole)
    {
        IQueryable<User> query;

        if (currentUserRole is Role.Administrator or Role.Moderator)
        {
            // Admin & modo voient tous les comptes
            query = _context.Users;
        }
        else if (currentUserRole == Role.SuperVendor)
        {
            // SuperVendor : uniquement vendeurs de sa(ces) marque(s)
            query =
                (from brand in _context.Brands
                    where brand.SuperVendorUserId == currentUserId
                    join brandSeller in _context.BrandSellers on brand.Id equals brandSeller.BrandId
                    join user in _context.Users on brandSeller.SellerId equals user.Id
                    where user.Role == Role.Vendor && brandSeller.IsActive
                    select user)
                .Distinct();
        }
        else
        {
            // Vendor / Client n’ont pas accès à cette liste
            throw new UnauthorizedAccessException("You are not allowed to view accounts.");
        }

        var accounts = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AccountDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                u.IsEnabled,
                u.IsInvitationPending
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