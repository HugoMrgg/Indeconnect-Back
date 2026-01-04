namespace IndeConnect_Back.Application.DTOs.Recommendations;

public record RecommendedProductDto(
    long Id,
    string Name,
    string? ImageUrl, 
    decimal BasePrice,
    decimal? SalePrice,
    string Description,
    double AverageRating,
    int ReviewsCount,
    string? BrandName,
    string? CategoryName,
    string? ColorName,
    double RecommendationScore,
    string RecommendationReason
);