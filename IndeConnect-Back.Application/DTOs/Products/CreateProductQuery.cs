using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record CreateProductQuery(
    string Name,
    string Description,
    decimal Price,
    long BrandId,
    long CategoryId,
    long? PrimaryColorId, 
    IEnumerable<ProductMediaDto> Media, 
    IEnumerable<ProductVariantDto> SizeVariants,
    IEnumerable<ProductDetailItemDto> Details,
    IEnumerable<string> Keywords,
    ProductStatus Status
    );