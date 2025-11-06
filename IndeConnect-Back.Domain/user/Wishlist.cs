namespace IndeConnect_Back.Domain.user;
/**
 * Represents a User's Wishlist
 */
public class Wishlist
{
    public long Id { get; private set; }

    public long UserId { get; private set; }
    public User User { get; private set; } = default!;
    private readonly List<WishlistItem> _items = new();
    public IReadOnlyCollection<WishlistItem> Items => _items;
    private Wishlist() {}

    public Wishlist(long userId) => UserId = userId;
}