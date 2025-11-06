namespace IndeConnect_Back.Domain;

public class Wishlist
{
    public long Id { get; private set; }

    public long UserId { get; private set; }
    public User User { get; private set; } = default!;

    private readonly HashSet<long> _productIds = new();
    public IReadOnlyCollection<long> ProductIds => _productIds;
    public IReadOnlyCollection<WishlistItem> Items { get; private set; } 

    private Wishlist() {} // EF

    public Wishlist(long userId) => UserId = userId;

    public bool Add(long productId) => _productIds.Add(productId);
    public bool Remove(long productId) => _productIds.Remove(productId);
}