using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record CreateProductResponse(
    long Id,
    string Name,
    string BrandName,
    string CategoryName,
    decimal Price,
    ProductStatus Status,
    DateTimeOffset CreatedAt
    );