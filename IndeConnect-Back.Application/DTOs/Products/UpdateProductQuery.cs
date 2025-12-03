using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record UpdateProductQuery(
    string Name,
    string Description,
    decimal Price,
    BrandSummaryDto Brand,
    CategoryDto Category,
    ColorDto? PrimaryColor, 
    IEnumerable<ProductMediaDto> Media, 
    IEnumerable<ProductVariantDto> SizeVariants,
    IEnumerable<ProductDetailItemDto> Details,
    IEnumerable<string> Keywords,
    ProductStatus Status,
    DateTimeOffset CreatedAt
);