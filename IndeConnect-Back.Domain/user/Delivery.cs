using IndeConnect_Back.Domain.order;

namespace IndeConnect_Back.Domain.user;
/**
 * Represents a User's delivry, with all the informations needed for a user to follow it.
 */

public class Delivery
{
    public long Id { get; private set; }
    public string Description { get; private set; } = default!; 
    public string? TrackingNumber { get; private set; } 
    
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ShippedAt { get; private set; } 
    public DateTimeOffset? DeliveredAt { get; private set; } 
    public DateTimeOffset UpdatedAt { get; private set; } 
    
    public DeliveryStatus Status { get; private set; } = DeliveryStatus.Pending; 

    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;

    private Delivery() { }

    public Delivery(string description, long orderId, string? trackingNumber = null)
    {
        Description = description;
        OrderId = orderId;
        TrackingNumber = trackingNumber;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsPreparing()
    {
        Status = DeliveryStatus.Preparing;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsShipped(DateTime shippedAt, string trackingNumber)
    {
        ShippedAt = shippedAt;
        TrackingNumber = trackingNumber;
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
