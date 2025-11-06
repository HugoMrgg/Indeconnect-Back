using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain.user;
/**
 * Represents a subscription of a user to a brand.
 */
public class BrandSubscription
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public User User { get; private set; } = default!;
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;
    public DateTimeOffset SubscribedAt { get; private set; }

    private BrandSubscription() { }

    public BrandSubscription(long userId, long brandId)
    {
        UserId = userId;
        BrandId = brandId;
        SubscribedAt = DateTimeOffset.UtcNow;
    }
}