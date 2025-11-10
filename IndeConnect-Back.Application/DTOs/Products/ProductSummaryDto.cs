namespace IndeConnect_Back.Application.DTOs.Products;


/// <summary>
/// Summary information for a product in list view
/// </summary>
public record ProductSummaryDto(
    long Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    string? Description,
    double AverageRating,
    int ReviewsCount
);
