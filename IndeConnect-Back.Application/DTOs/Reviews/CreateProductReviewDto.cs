namespace IndeConnect_Back.Application.DTOs.Reviews;

public record CreateProductReviewDto(
    int Rating,
    string? Comment
);