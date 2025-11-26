using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

/**
 * Service handling all wishlist-related operations, including retrieval and update logic.
 */
public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;
    private readonly IAuditTrailService _auditTrail;

    public WishlistService(AppDbContext context, IAuditTrailService auditTrail)
    {
        _context = context;
        _auditTrail = auditTrail;
    }

    /**
     * Retrieves the wishlist of the given user, creating an empty wishlist if none exists.
     */
    public async Task<WishlistDto> GetUserWishlistAsync(long userId)
    {
        var user = await _context.Users
            .Include(u => u.Wishlist)
                .ThenInclude(w => w!.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Brand)
            .Include(u => u.Wishlist)
                .ThenInclude(w => w!.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Variants)
                            .ThenInclude(v => v.Media)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("User not found");

        // If the user does not have a wishlist yet, create an empty one
        if (user.Wishlist == null)
        {
            var wishlist = new Wishlist(userId);
            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();
            
            return new WishlistDto(
                wishlist.Id,
                userId,
                Enumerable.Empty<WishlistItemDto>(),
                0
            );
        }

        // Map wishlist products to DTOs with info and main image
        var items = user.Wishlist.Items.Select(item =>
        {
            var product = item.Product;
            
            // Get the primary image of the first available variant; fall back to first image
            var primaryImage = product.Variants
                .SelectMany(v => v.Media)
                .Where(m => m.IsPrimary)
                .Select(m => m.Url)
                .FirstOrDefault();
            
            if (primaryImage == null)
            {
                primaryImage = product.Variants
                    .SelectMany(v => v.Media)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => m.Url)
                    .FirstOrDefault();
            }
            
            // Determine if any variant is in stock
            var hasStock = product.Variants.Any(v => v.StockCount > 0);

            return new WishlistItemDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Brand.Name,
                product.CategoryId,
                primaryImage,
                hasStock,
                item.AddedAt
            );
        });

        return new WishlistDto(
            user.Wishlist.Id,
            userId,
            items,
            user.Wishlist.Items.Count
        );
    }

    /**
     * Adds a product to the user's wishlist, creating the wishlist if needed.
     */
    public async Task<WishlistDto> AddProductToWishlistAsync(long userId, long productId)
    {
        var user = await _context.Users
            .Include(u => u.Wishlist)
                .ThenInclude(w => w!.Items)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("User not found");

        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == productId);
            
        if (product == null)
            throw new InvalidOperationException("Product not found");

        // Check that the product is active/enabled
        if (!product.IsEnabled)
            throw new InvalidOperationException("Product is not available");

        // Create wishlist if it doesn't exist
        if (user.Wishlist == null)
        {
            var newWishlist = new Wishlist(userId);
            _context.Wishlists.Add(newWishlist);
            await _context.SaveChangesAsync();
            
            // Reload the user to attach the new wishlist
            user = await _context.Users
                .Include(u => u.Wishlist)
                    .ThenInclude(w => w!.Items)
                .FirstAsync(u => u.Id == userId);
        }

        // Prevent duplicates: do not add if product is already in wishlist
        if (user.Wishlist!.Items.Any(i => i.ProductId == productId))
            throw new InvalidOperationException("Product is already in wishlist");

        var wishlistItem = new WishlistItem(user.Wishlist.Id, productId);
        _context.WishlistItems.Add(wishlistItem);
        await _auditTrail.LogAsync(
            action: "WishlistAdd",
            userId: userId,
            details: $"{user.FirstName} {user.LastName} has added {product.Name} in his wishlist"
        );
        await _context.SaveChangesAsync();

        return await GetUserWishlistAsync(userId);
    }

    /**
     * Removes a product from the user's wishlist.
     */
    public async Task RemoveProductFromWishlistAsync(long userId, long productId)
    {
        var user = await _context.Users
            .Include(u => u.Wishlist)
                .ThenInclude(w => w!.Items)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("User not found");

        if (user.Wishlist == null)
            throw new InvalidOperationException("Wishlist not found");

        var item = user.Wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException("Product not found in wishlist");

        _context.WishlistItems.Remove(item);
        await _auditTrail.LogAsync(
            action: "WishlistAdd",
            userId: userId,
            details: $"{user.FirstName} {user.LastName} has removed product{productId} in his wishlist"
        );
        await _context.SaveChangesAsync();
    }

    /**
     * Checks if a product is present in a user's wishlist.
     */
    public async Task<bool> IsProductInWishlistAsync(long userId, long productId)
    {
        return await _context.WishlistItems
            .AnyAsync(wi => wi.Wishlist.UserId == userId && wi.ProductId == productId);
    }
}
