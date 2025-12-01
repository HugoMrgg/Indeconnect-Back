namespace IndeConnect_Back.Application.DTOs.Brands;

public record BrandSummaryDto(
    long Id,
    string Name,
    string? LogoUrl,
    string? BannerUrl,
    string? Description,
    double EthicsScoreProduction,
    double EthicsScoreTransport,
    IEnumerable<string> EthicTags,
    string? Address,
    int? DistanceKm,
    double UserRating
);