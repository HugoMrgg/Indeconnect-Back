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

    /// <summary>
    /// Construit la timeline de suivi pour cette livraison.
    /// Encapsule le workflow métier de suivi de commande avec ses différentes étapes.
    /// </summary>
    /// <param name="order">La commande parente pour accéder au statut de paiement</param>
    /// <returns>Liste des étapes de la timeline dans l'ordre</returns>
    public List<DeliveryTrackingStep> BuildTrackingTimeline(Order order)
    {
        var timeline = new List<DeliveryTrackingStep>();

        // Étape 1: Commande passée (toujours complétée)
        timeline.Add(new DeliveryTrackingStep(
            status: "Placed",
            label: "Commande passée",
            description: "Votre commande a été enregistrée",
            completedAt: order.PlacedAt,
            isCompleted: true,
            isCurrent: false
        ));

        // Étape 2: Paiement confirmé
        var isPaid = order.Status >= OrderStatus.Paid;
        timeline.Add(new DeliveryTrackingStep(
            status: "Paid",
            label: "Paiement confirmé",
            description: "Votre paiement a été accepté",
            completedAt: isPaid ? order.PlacedAt : null,
            isCompleted: isPaid,
            isCurrent: false
        ));

        // Étape 3: En préparation
        var preparingCompleted = Status > DeliveryStatus.Preparing;
        var preparingCurrent = (order.Status == OrderStatus.Paid && Status == DeliveryStatus.Pending)
                             || (order.Status == OrderStatus.Processing && Status == DeliveryStatus.Preparing);
        var preparingDescription = order.Status == OrderStatus.Paid && Status == DeliveryStatus.Pending
            ? "Votre commande va bientôt être préparée"
            : "Votre commande est en cours de préparation";

        timeline.Add(new DeliveryTrackingStep(
            status: "Preparing",
            label: "En préparation",
            description: preparingDescription,
            completedAt: preparingCompleted ? CreatedAt : null,
            isCompleted: preparingCompleted,
            isCurrent: preparingCurrent
        ));

        // Étape 4: Expédiée
        var shippedCompleted = Status > DeliveryStatus.Shipped;
        var shippedCurrent = Status == DeliveryStatus.Shipped;

        timeline.Add(new DeliveryTrackingStep(
            status: "Shipped",
            label: "Expédiée",
            description: "Votre colis a été pris en charge par le transporteur",
            completedAt: shippedCompleted || shippedCurrent ? ShippedAt : null,
            isCompleted: shippedCompleted,
            isCurrent: shippedCurrent
        ));

        // Étape 5: En transit
        var inTransitCompleted = Status > DeliveryStatus.InTransit;
        var inTransitCurrent = Status == DeliveryStatus.InTransit;

        timeline.Add(new DeliveryTrackingStep(
            status: "InTransit",
            label: "En transit",
            description: "Votre colis est en cours d'acheminement",
            completedAt: inTransitCompleted || inTransitCurrent ? UpdatedAt : null,
            isCompleted: inTransitCompleted,
            isCurrent: inTransitCurrent
        ));

        // Étape 6: En cours de livraison
        var outForDeliveryCompleted = Status == DeliveryStatus.Delivered;
        var outForDeliveryCurrent = Status == DeliveryStatus.OutForDelivery;

        timeline.Add(new DeliveryTrackingStep(
            status: "OutForDelivery",
            label: "En cours de livraison",
            description: "Votre colis est en cours de livraison",
            completedAt: outForDeliveryCompleted || outForDeliveryCurrent ? UpdatedAt : null,
            isCompleted: outForDeliveryCompleted,
            isCurrent: outForDeliveryCurrent
        ));

        // Étape 7: Livrée
        var deliveredCompleted = Status == DeliveryStatus.Delivered;

        timeline.Add(new DeliveryTrackingStep(
            status: "Delivered",
            label: "Livrée",
            description: "Votre commande a été livrée avec succès",
            completedAt: DeliveredAt,
            isCompleted: deliveredCompleted,
            isCurrent: deliveredCompleted
        ));

        return timeline;
    }
}
