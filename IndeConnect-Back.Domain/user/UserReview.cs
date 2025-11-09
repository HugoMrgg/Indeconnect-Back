using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain.user;

public class UserReview
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public User User { get; private set; } = default!;
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private UserReview() { }
    public UserReview(long userId, long brandId, int rating, string? comment)
    {
        UserId = userId;
        BrandId = brandId;
        Rating = rating;
        Comment = comment?.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}