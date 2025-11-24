namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductColorVariantDto(
    long ProductId,
    long? ColorId,
    string? ColorName,
    string? ColorHexa,
    string? ThumbnailUrl,
    bool IsAvailable
);