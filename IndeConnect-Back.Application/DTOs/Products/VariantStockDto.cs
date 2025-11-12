namespace IndeConnect_Back.Application.DTOs.Products;

public record VariantStockDto(
    long VariantId,
    string SKU,
    int Stock,
    bool IsAvailable
);