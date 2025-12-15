namespace IndeConnect_Back.Application.DTOs.Brands;

public record CreateShippingMethodDto
{
    public string ProviderName { get; init; } = default!;
    public string MethodType { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public decimal Price { get; init; }
    public int EstimatedMinDays { get; init; }
    public int EstimatedMaxDays { get; init; }
    public decimal? MaxWeight { get; init; }
    public string? ProviderConfig { get; init; }
}