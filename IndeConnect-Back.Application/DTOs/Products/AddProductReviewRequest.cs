namespace IndeConnect_Back.Application.DTOs.Products;

public record AddProductReviewRequest(
    int Rating,
    string? Comment
);
