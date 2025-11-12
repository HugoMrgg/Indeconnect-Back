namespace IndeConnect_Back.Application.DTOs.Products;


/**
 * Summary information for a product in list view
 */
public record ProductSummaryDto(
    long Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    string? Description,
    double AverageRating,
    int ReviewsCount
);
