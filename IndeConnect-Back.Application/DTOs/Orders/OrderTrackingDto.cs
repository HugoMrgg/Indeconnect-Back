using IndeConnect_Back.Domain.order;

namespace IndeConnect_Back.Application.DTOs.Orders;

public class OrderTrackingDto
{
    public long OrderId { get; set; }
    public OrderStatus GlobalStatus { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public decimal TotalAmount { get; set; }

    // Tracking par marque - chaque marque a sa propre livraison
    public List<BrandDeliveryTrackingDto> DeliveriesByBrand { get; set; } = new();

    // Date de livraison estimée globale (la plus tardive de toutes les marques)
    public DateTimeOffset? LatestEstimatedDelivery { get; set; }
}