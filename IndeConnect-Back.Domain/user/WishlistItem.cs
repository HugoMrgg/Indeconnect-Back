using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Domain.user;

public class WishlistItem
{
    // Clé composite
    public long WishlistId { get; private set; }
    public Wishlist Wishlist { get; private set; } = default!;
    
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    
    public DateTimeOffset AddedAt { get; private set; }

    private WishlistItem() { }

    public WishlistItem(long wishlistId, long productId)
    {
        WishlistId = wishlistId;
        ProductId = productId;
        AddedAt = DateTimeOffset.UtcNow;
    }
}
