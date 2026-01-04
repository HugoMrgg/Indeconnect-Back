using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Application.DTOs.Brands;

public record BrandDetailDto(
    long Id,
    string Name,
    string? LogoUrl,
    string? BannerUrl,
    string? Description,
    string? AboutUs,
    string? WhereAreWe,
    string? OtherInfo,
    string? Contact,
    string? PriceRange,
    double AverageUserRating,
    int ReviewsCount,
    IEnumerable<string> EthicTags,
    IEnumerable<DepositDto> Deposits,
    double EthicsScoreProduction,
    double EthicsScoreTransport,
    string? AccentColor,
    BrandStatus? Status = null,
    string? LatestRejectionComment = null
);

