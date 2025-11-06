using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * Represents the association between a Brand and a Seller (User).
 * A brand can have multiple sellers, and a user can be a seller for multiple brands.
 */

public class BrandSeller
{
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    public long SellerId { get; private set; }
    public User Seller { get; private set; } = default!;

    public DateTimeOffset JoinedAt { get; private set; }
    public bool IsActive { get; private set; }

    private BrandSeller() { }

    public BrandSeller(long brandId, long sellerId)
    {
        BrandId = brandId;
        SellerId = sellerId;
        JoinedAt = DateTimeOffset.UtcNow;
        IsActive = true;
    }
}