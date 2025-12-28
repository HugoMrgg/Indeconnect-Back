using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record UpdateProductQuery(
    string Name,
    string Description,
    decimal Price,
    ProductStatus Status,
    long? PrimaryColorId,
    long CategoryId,
    UpdateProductSaleDto? Sale,
    List<UpdateProductVariantDto> Variants,
    List<UpdateProductMediaDto> Media
);

public record UpdateProductSaleDto(
    decimal DiscountPercentage,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? Description
);

public record UpdateProductVariantDto(
    long SizeId,
    int StockCount,
    long? Id // Si présent = mise à jour, sinon = création
);

public record UpdateProductMediaDto(
    string Url,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);