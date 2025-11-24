using IndeConnect_Back.Application.DTOs.Brands;

namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductGroupDto(
    long Id,
    string Name,
    string BaseDescription,
    BrandSummaryDto Brand,
    CategoryDto Category,
    IEnumerable<ProductColorVariantDto> ColorVariants
);