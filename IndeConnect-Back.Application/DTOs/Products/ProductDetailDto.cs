using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductDetailDto(
    long Id,
    string Name,
    string Description,
    decimal Price,
    decimal? SalePrice,
    SaleDto? Sale,
    BrandSummaryDto Brand,
    CategoryDto Category,
    ColorDto? PrimaryColor, 
    IEnumerable<ProductColorVariantDto> ColorVariants, 
    IEnumerable<ProductMediaDto> Media, 
    IEnumerable<ProductVariantDto> SizeVariants,
    IEnumerable<ProductDetailItemDto> Details,
    IEnumerable<string> Keywords,
    IEnumerable<ProductReviewDto> Reviews,
    double AverageRating,
    int ReviewCount,
    int TotalStock,
    bool IsAvailable,
    ProductStatus Status,
    DateTimeOffset CreatedAt
);