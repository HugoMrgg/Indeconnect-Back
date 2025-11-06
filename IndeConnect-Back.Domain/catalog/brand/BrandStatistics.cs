namespace IndeConnect_Back.Domain.catalog.brand;

public class BrandStatistics
{
    public long Id { get; private set; }
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    public int TotalProducts { get; private set; }
    public int OnlineProducts { get; private set; }
    public int SalesCount { get; private set; }
    public decimal TotalTurnover { get; private set; } // Chiffre d'affaires total
    public int ReviewCount { get; private set; }
    public double AverageReview { get; private set; }

    public DateTime LastComputedAt { get; private set; }

    private BrandStatistics() { }

    public BrandStatistics(long brandId)
    {
        BrandId = brandId;
        LastComputedAt = DateTime.UtcNow;
    }

    public void Update(
        int totalProducts,
        int onlineProducts,
        int salesCount,
        decimal totalTurnover,
        int reviewCount,
        double averageReview)
    {
        TotalProducts = totalProducts;
        OnlineProducts = onlineProducts;
        SalesCount = salesCount;
        TotalTurnover = totalTurnover;
        ReviewCount = reviewCount;
        AverageReview = averageReview;
        LastComputedAt = DateTime.UtcNow;
    }
}