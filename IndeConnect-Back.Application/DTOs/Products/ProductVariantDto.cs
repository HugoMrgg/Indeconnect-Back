namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductVariantDto(
    string SKU,
    SizeDto? Size,
    int StockCount,
    decimal Price,
    bool IsAvailable
);