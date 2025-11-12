using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;

    public WishlistService(AppDbContext context)
    {
        _context = context;
    }

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

        var items = user.Wishlist.Items.Select(item =>
        {
            var product = item.Product;
            
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

        // Vérifier que le produit est activé
        if (!product.IsEnabled)
            throw new InvalidOperationException("Product is not available");

        // Créer la wishlist si elle n'existe pas
        if (user.Wishlist == null)
        {
            var newWishlist = new Wishlist(userId);
            _context.Wishlists.Add(newWishlist);
            await _context.SaveChangesAsync();
            
            // Recharger l'utilisateur avec la nouvelle wishlist
            user = await _context.Users
                .Include(u => u.Wishlist)
                    .ThenInclude(w => w!.Items)
                .FirstAsync(u => u.Id == userId);
        }

        // Vérifier si le produit est déjà dans la wishlist
        if (user.Wishlist!.Items.Any(i => i.ProductId == productId))
            throw new InvalidOperationException("Product is already in wishlist");

        var wishlistItem = new WishlistItem(user.Wishlist.Id, productId);
        _context.WishlistItems.Add(wishlistItem);
        await _context.SaveChangesAsync();

        return await GetUserWishlistAsync(userId);
    }

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
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsProductInWishlistAsync(long userId, long productId)
    {
        return await _context.WishlistItems
            .AnyAsync(wi => wi.Wishlist.UserId == userId && wi.ProductId == productId);
    }
}
