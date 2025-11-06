namespace IndeConnect_Back.Domain;

public class Delivery
{
    public long Id { get; private set; }
    public string Description { get; private set; } = default!; // "BPost", "Pickup", etc.

    public string? TrackingNumber { get; private set; } // Numéro de suivi (fourni par BPost)
    public DateTime? ShippedAt { get; private set; } // Date d’expédition
    public DeliveryStatus Status { get; private set; } = DeliveryStatus.Pending; // Enumération d’états

    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;

    private Delivery() { }

    // Constructeur pour nouvelle livraison
    public Delivery(string description, long orderId, string? trackingNumber = null)
    {
        Description = description;
        OrderId = orderId;
        TrackingNumber = trackingNumber;
    }

    // Méthodes métier : changement de statut
    public void MarkAsShipped(DateTime shippedAt, string trackingNumber)
    {
        ShippedAt = shippedAt;
        TrackingNumber = trackingNumber;
        Status = DeliveryStatus.Shipped;
    }

    public void MarkAsDelivered()
    {
        Status = DeliveryStatus.Delivered;
    }

    public void MarkAsCancelled()
    {
        Status = DeliveryStatus.Cancelled;
    }
}

