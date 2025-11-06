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
    public DateTime CreatedAt { get; private set; }
    public ReviewStatus Status { get; private set; } = ReviewStatus.Pending; // modération

    private ProductReview() { } // EF

    public ProductReview(long productId, long userId, int rating, string? comment)
    {
        if (rating < 1 || rating > 5) throw new ArgumentOutOfRangeException(nameof(rating));
        ProductId = productId;
        UserId = userId;
        Rating = rating;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        CreatedAt = DateTime.UtcNow;
        Status = ReviewStatus.Pending;
    }

    public void Approve() => Status = ReviewStatus.Approved;
    public void Reject() => Status = ReviewStatus.Rejected;
}
