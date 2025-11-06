using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Domain.catalog.product;

public class ProductReview
{
    public long Id { get; private set; }
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public long UserId { get; private set; }
    public User User { get; private set; } = default!;

    public int Rating { get; private set; } // 1 à 5
    public string? Comment { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public ReviewStatus Status { get; private set; } = ReviewStatus.Pending;

    private ProductReview() { }

    public ProductReview(long productId, long userId, int rating, string? comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5");
            
        ProductId = productId;
        UserId = userId;
        Rating = rating;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
        Status = ReviewStatus.Pending;
    }
}
