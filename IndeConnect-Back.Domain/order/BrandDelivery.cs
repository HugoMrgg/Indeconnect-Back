using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Domain.order;

/// <summary>
/// Represents a delivery for a specific brand within an order.
/// Each order can have multiple brand deliveries, one per brand.
/// This allows independent tracking and progression for each brand's items.
/// </summary>
public class BrandDelivery
{
    public long Id { get; private set; }

    // Brand relationship
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    // Order relationship
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;

    // Shipping method chosen by the customer
    public long? ShippingMethodId { get; private set; }
    public BrandShippingMethod? ShippingMethod { get; private set; }

    // Shipping fee based on the chosen method
    public decimal ShippingFee { get; private set; } = 0m;

    // Delivery items from this brand
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items;

    // Tracking information
    public string Description { get; private set; } = default!;
    public string? TrackingNumber { get; private set; }
    public DeliveryStatus Status { get; private set; } = DeliveryStatus.Pending;

    // Dates
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ShippedAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? EstimatedDelivery { get; private set; }

    private BrandDelivery() { }

    public BrandDelivery(
        long brandId,
        long orderId,
        string description,
        long? shippingMethodId = null,
        decimal shippingFee = 0m,
        string? trackingNumber = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        BrandId = brandId;
        OrderId = orderId;
        Description = description.Trim();
        ShippingMethodId = shippingMethodId;
        ShippingFee = shippingFee;
        TrackingNumber = trackingNumber;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddItem(OrderItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _items.Add(item);
    }

    public void SetEstimatedDelivery(DateTimeOffset estimatedDelivery)
    {
        EstimatedDelivery = estimatedDelivery;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsPreparing()
    {
        Status = DeliveryStatus.Preparing;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsShipped(DateTime shippedAt, string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required", nameof(trackingNumber));

        ShippedAt = shippedAt;
        TrackingNumber = trackingNumber.Trim();
        Status = DeliveryStatus.Shipped;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsInTransit()
    {
        Status = DeliveryStatus.InTransit;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsOutForDelivery()
    {
        Status = DeliveryStatus.OutForDelivery;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsDelivered()
    {
        Status = DeliveryStatus.Delivered;
        DeliveredAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsCancelled()
    {
        Status = DeliveryStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
