using IndeConnect_Back.Application.DTOs.Products;

namespace IndeConnect_Back.Application.DTOs.Users;

public record CartItemDto(
    long ProductId,
    string ProductName,
    string BrandName,
    long BrandId,
    string? PrimaryImageUrl,
    ColorDto? Color,
    SizeDto? Size,
    long ProductVariantId,
    string SKU,
    int AvailableStock,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    DateTimeOffset AddedAt
);