using IndeConnect_Back.Domain.payment;

namespace IndeConnect_Back.Domain.order;

public class Order
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public User User { get; private set; } = default!;

    public DateTime PlacedAt { get; private set; } = DateTime.UtcNow;
    public string Currency { get; private set; } = "EUR";
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    // Paiement
    public Payment Payment { get; private set; }

    // Lignes de commande
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items;

    // Livraison (cf multi-marques, peut devenir collection)
    public Delivery Delivery { get; private set; }

    // Facture
    public Invoice Invoice { get; private set; }

    // Retours/RMA (optionnel, mais important pour l'e-commerce sérieux)
    private readonly List<ReturnRequest> _returns = new();
    public IReadOnlyCollection<ReturnRequest> Returns => _returns;

    private Order() { }
    public Order(long userId, IEnumerable<OrderItem> items, decimal totalAmount, string? currency = null)
    {
        UserId = userId;
        _items.AddRange(items);
        TotalAmount = totalAmount;
        Currency = currency ?? "EUR";
        Status = OrderStatus.Pending;
        PlacedAt = DateTime.UtcNow;
    }

    public void MarkPaid() => Status = OrderStatus.Paid;
    public void MarkDelivered() => Status = OrderStatus.Delivered;
    public void MarkCancelled() => Status = OrderStatus.Cancelled;
}
