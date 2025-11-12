namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductStockDto(
    long ProductId,
    int TotalStock,
    bool IsAvailable,
    IEnumerable<VariantStockDto> VariantStocks
);