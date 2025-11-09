namespace IndeConnect_Back.Application.DTOs.Brands;

public record BrandSummaryDto(
    long Id,
    string Name,
    string? LogoUrl,
    string? Description,
    double EthicsScore,
    IEnumerable<string> EthicTags,
    int? DistanceKm,
    double UserRating  
);