namespace IndeConnect_Back.Domain.order;

public class OrderItem
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public string Name { get; private set; } = default!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    // Optionnel : gestion taille/couleur au moment de commande
    public long? SizeId { get; private set; }
    public string? ColorHexa { get; private set; }

    private OrderItem() { }
    public OrderItem(long orderId, long productId, string name, int quantity, decimal unitPrice, long? sizeId = null, string? colorHexa = null)
    {
        OrderId = orderId;
        ProductId = productId;
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
        SizeId = sizeId;
        ColorHexa = colorHexa;
    }
}