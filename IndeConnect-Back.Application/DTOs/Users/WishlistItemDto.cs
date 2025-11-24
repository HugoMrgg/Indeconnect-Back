using IndeConnect_Back.Application.DTOs.Products;

namespace IndeConnect_Back.Application.DTOs.Users;

public record WishlistItemDto(
    long ProductId,
    string ProductName,
    string Description,
    decimal Price,
    string BrandName,
    long CategoryId,
    string? PrimaryImageUrl,
    bool HasStock,
    DateTimeOffset AddedAt,
    ColorDto? PrimaryColor 
);