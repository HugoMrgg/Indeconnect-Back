using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Products;

public record ProductReviewDto(
    long Id,
    long UserId,
    string UserName,
    int Rating,
    string? Comment,
    DateTimeOffset CreatedAt,
    ReviewStatus Status
);