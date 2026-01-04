using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductSummaryDto(
    long Id,
    string Name,
    string? PrimaryImageUrl,
    decimal Price,
    string Description,
    double AverageRating,
    int ReviewCount,
    ColorDto? PrimaryColor,
    ProductStatus Status,
    SaleDto? Sale
);