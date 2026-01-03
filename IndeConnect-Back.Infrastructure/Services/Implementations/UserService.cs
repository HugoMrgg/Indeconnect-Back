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

    public async Task<UserDetailDto?> GetUserByIdAsync(long? userId)
    {
        var user = await _context.Users
            .Include(u => u.BrandSubscriptions)
            .Include(u => u.Reviews)
            .Include(u => u.Orders)
            .Include(u => u.Brand) 
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
            user.Orders.Count,
            user.Role == Role.SuperVendor 
                ? user.BrandId
                : null
        );
    }

    public async Task<List<AccountDto>> GetAllAccountsAsync(long? currentUserId, Role currentUserRole)
    {
        IQueryable<User> query;

        if (currentUserRole is Role.Administrator or Role.Moderator)
        {
            query = _context.Users;
        }
        else if (currentUserRole == Role.SuperVendor)
        {
            // Récupérer le BrandId du SuperVendor
            var superVendorBrandId = await _context.Users
                .Where(u => u.Id == currentUserId)
                .Select(u => u.BrandId)
                .FirstOrDefaultAsync();

            if (superVendorBrandId == null)
            {
                // Le SuperVendor n'a pas de marque, retourner liste vide
                return new List<AccountDto>();
            }

            // Récupérer les Vendors actifs de cette marque via BrandSellers
            query = _context.Users
                .Where(u => u.Role == Role.Vendor)
                .Where(u => _context.BrandSellers.Any(bs => 
                    bs.SellerId == u.Id && 
                    bs.BrandId == superVendorBrandId && 
                    bs.IsActive
                ));
        }

        else
        {
            // Vendor / Client n'ont pas accès à cette liste
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
