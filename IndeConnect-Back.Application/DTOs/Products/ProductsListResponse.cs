namespace IndeConnect_Back.Application.DTOs.Products;

/// <summary>
/// Response for paginated products list
/// </summary>
public record ProductsListResponse(
    IEnumerable<ProductSummaryDto> Products,
    int TotalCount,
    int Page,
    int PageSize
);
