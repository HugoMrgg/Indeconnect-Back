namespace IndeConnect_Back.Domain;

public class Cart
{
    public long Id { get; private set; }

    public long UserId { get; private set; }
    public User User { get; private set; } = default!;

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private Cart() {} // EF

    public Cart(long userId)
    {
        UserId = userId;
    }

    public void AddItem(long productId, int quantity, decimal unitPrice, Product product)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is null)
            _items.Add(new CartItem(Id, productId, quantity, unitPrice, product));
        else
            existing.Increase(quantity);

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(long productId)
    {
        var it = _items.FirstOrDefault(i => i.ProductId == productId);
        if (it is null) return;
        _items.Remove(it);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetQuantity(long productId, int quantity)
    {
        var it = _items.FirstOrDefault(i => i.ProductId == productId)
                 ?? throw new InvalidOperationException("Item not in cart.");
        it.SetQuantity(quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
