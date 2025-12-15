using IndeConnect_Back.Domain.payment;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Domain.order;
/**
 * Represents a User's Order
 */
public class Order
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public User User { get; private set; } = default!;

    public long ShippingAddressId { get; private set; }
    public ShippingAddress ShippingAddress { get; private set; } = default!;

    public DateTimeOffset PlacedAt { get; private set; }
    public string Currency { get; private set; } = "EUR";
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public long? PaymentId { get; private set; }
    public Payment? Payment { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items;

    private readonly List<Delivery> _deliveries = new();
    public IReadOnlyCollection<Delivery> Deliveries => _deliveries;

    private readonly List<Invoice> _invoices = new();
    public IReadOnlyCollection<Invoice> Invoices => _invoices;

    private readonly List<ReturnRequest> _returns = new();
    public IReadOnlyCollection<ReturnRequest> Returns => _returns;

    private Order() { }
    
    public Order(long userId, long shippingAddressId, IEnumerable<OrderItem> items, string? currency = null)
    {
        if (items == null || !items.Any())
            throw new ArgumentException("Order must have at least one item", nameof(items));
            
        UserId = userId;
        ShippingAddressId = shippingAddressId;
        _items.AddRange(items);
        Currency = currency ?? "EUR";
        Status = OrderStatus.Pending;
        PlacedAt = DateTimeOffset.UtcNow;
        
        TotalAmount = _items.Sum(item => item.Quantity * item.UnitPrice);
    }
}
