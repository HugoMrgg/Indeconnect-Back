using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Domain.user;
/**
 * Represents a User's Cart, a Cart has multiple CartItem
 */
public class Cart
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public User User { get; private set; } = default!;

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items;

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Cart() {}

    public Cart(long userId)
    {
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
