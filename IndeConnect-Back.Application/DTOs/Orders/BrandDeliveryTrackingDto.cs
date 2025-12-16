using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Application.DTOs.Orders;

/// <summary>
/// Represents tracking information for a specific brand's delivery within an order.
/// </summary>
public class BrandDeliveryTrackingDto
{
    public long BrandDeliveryId { get; set; }
    public long BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? BrandLogoUrl { get; set; }

    public DeliveryStatus Status { get; set; }
    public string? TrackingNumber { get; set; }

    // Items from this brand
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }

    // Dates
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? EstimatedDelivery { get; set; }

    // Timeline of tracking steps for this brand's delivery
    public List<TrackingStepDto> Timeline { get; set; } = new();
}
