namespace IndeConnect_Back.Application.DTOs.Products;


/**
 * Query parameters for filtering and sorting products
 */
public record GetProductsQuery(
    long BrandId,
    int Page = 1,
    int PageSize = 20,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Category = null,
    string? SearchTerm = null,
    ProductSortType SortBy = ProductSortType.Newest
);

/**
 * Sort options for products
 */
public enum ProductSortType
{
    Newest, // Most recent first
    PriceAsc, // Cheapest first
    PriceDesc, // Most expensive first
    Rating, // Highest rated first
    Popular // Most reviewed first
}