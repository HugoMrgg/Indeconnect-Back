namespace IndeConnect_Back.Application.DTOs.Products;

/**
 * Response for paginated products list
 */
public record ProductsListResponse(
    IEnumerable<ProductSummaryDto> Products,
    int TotalCount,
    int Page,
    int PageSize
);
