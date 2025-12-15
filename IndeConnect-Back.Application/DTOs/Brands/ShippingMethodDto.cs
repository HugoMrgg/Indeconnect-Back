namespace IndeConnect_Back.Application.DTOs.Brands;

public record ShippingMethodDto
{
    public long Id { get; init; }
    public string ProviderName { get; init; } = default!;
    public string MethodType { get; init; } = default!; // "HomeDelivery", "Locker", etc.
    public string DisplayName { get; init; } = default!;
    public decimal Price { get; init; }
    public int EstimatedMinDays { get; init; }
    public int EstimatedMaxDays { get; init; }
    public decimal? MaxWeight { get; init; }
    public bool IsEnabled { get; init; }
}