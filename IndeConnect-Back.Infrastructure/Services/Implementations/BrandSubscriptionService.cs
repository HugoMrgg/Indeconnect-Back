using IndeConnect_Back.Application.DTOs.Subscriptions;
using IndeConnect_Back.Application.Services.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class BrandSubscriptionService : IBrandSubscriptionService
{
    private readonly AppDbContext _context;

    public BrandSubscriptionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BrandSubscriptionResponse> SubscribeToBrandAsync(long userId, long brandId)
    {
        var user = await _context.Users
            .Include(u => u.BrandSubscriptions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("User not found");

        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == brandId);

        if (brand == null)
            throw new InvalidOperationException("Brand not found");

        if (user.IsSubscribedToBrand(brandId))
            throw new InvalidOperationException($"Already subscribed to {brand.Name}");

        user.SubscribeToBrand(brand);

        await _context.SaveChangesAsync();

        var subscription = user.BrandSubscriptions.First(bs => bs.BrandId == brandId);

        return new BrandSubscriptionResponse(
            subscription.Id,
            subscription.UserId,
            subscription.BrandId,
            brand.Name,
            subscription.SubscribedAt
        );
    }

    public async Task<UserBrandSubscriptionsResponse> GetUserSubscriptionsAsync(long userId)
    {
        var user = await _context.Users
            .Include(u => u.BrandSubscriptions)
                .ThenInclude(bs => bs.Brand)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("User not found");

        var subscriptionItems = user.BrandSubscriptions
            .Select(bs => new BrandSubscriptionItem(
                bs.BrandId,
                bs.Brand.Name,
                bs.Brand.LogoUrl,
                bs.SubscribedAt
            ))
            .OrderByDescending(s => s.SubscribedAt)
            .ToList();

        return new UserBrandSubscriptionsResponse(userId, subscriptionItems);
    }

    public async Task UnsubscribeFromBrandAsync(long userId, long brandId)
    {
        var user = await _context.Users
            .Include(u => u.BrandSubscriptions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("User not found");

        user.UnsubscribeFromBrand(brandId);

        await _context.SaveChangesAsync();
    }
}
