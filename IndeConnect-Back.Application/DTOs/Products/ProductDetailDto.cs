using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductDetailDto(
    long Id,
    string Name,
    string Description,
    decimal BasePrice,
    decimal? SalePrice,
    SaleDto? Sale,
    BrandSummaryDto Brand,
    CategoryDto Category,
    IEnumerable<ProductVariantDto> Variants,
    IEnumerable<ProductDetailItemDto> Details,
    IEnumerable<string> Keywords,
    IEnumerable<ProductReviewDto> Reviews,
    double AverageRating,
    int ReviewsCount,
    int TotalStock,
    bool IsAvailable,
    ProductStatus Status,
    DateTimeOffset CreatedAt
);