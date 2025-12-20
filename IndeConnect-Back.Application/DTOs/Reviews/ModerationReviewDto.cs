using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Application.DTOs.Reviews;

public record ModerationReviewDto(
    long Id,
    long ProductId,
    string ProductName,
    long UserId,
    string UserDisplayName,
    int Rating,
    string? Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    ReviewStatus Status
);