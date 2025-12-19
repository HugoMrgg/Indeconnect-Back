namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductVariantDto(
    long Id,
    string SKU,
    SizeDto? Size,
    int StockCount,
    decimal Price,
    bool IsAvailable
);