using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
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
            user.Role.ToString(),
            user.BrandSubscriptions.Count,
            user.Reviews.Count,
            user.Orders.Count
        );
    }
}