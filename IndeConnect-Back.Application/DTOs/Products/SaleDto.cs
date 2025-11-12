namespace IndeConnect_Back.Application.DTOs.Products;

public record SaleDto(
    long Id,
    string Name,
    string Description,
    decimal DiscountPercentage,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    bool IsActive
);