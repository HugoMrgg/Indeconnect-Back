using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Domain.order;

public class OrderItem
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;
    
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    
    public long? VariantId { get; private set; }
    public ProductVariant? Variant { get; private set; }

    public long? BrandDeliveryId { get; private set; }
    public BrandDelivery? BrandDelivery { get; private set; }

    public string ProductName { get; private set; } = default!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private OrderItem() { }
    
    public OrderItem(long productId, string productName, int quantity, decimal unitPrice, long? variantId = null)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));
            
        ProductId = productId;
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        VariantId = variantId;
    }
    
    internal void SetOrderId(long orderId)
    {
        OrderId = orderId;
    }

    public void AssignToBrandDelivery(long brandDeliveryId)
    {
        BrandDeliveryId = brandDeliveryId;
    }
}