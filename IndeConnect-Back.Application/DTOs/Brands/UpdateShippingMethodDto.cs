namespace IndeConnect_Back.Application.DTOs.Brands;

public record UpdateShippingMethodDto
{
    public string? ProviderName { get; init; }
    public string? MethodType { get; init; }
    public string? DisplayName { get; init; }
    public decimal? Price { get; init; }
    public int? EstimatedMinDays { get; init; }
    public int? EstimatedMaxDays { get; init; }
    public decimal? MaxWeight { get; init; }
    public string? ProviderConfig { get; init; }
    public bool? IsEnabled { get; init; }
}