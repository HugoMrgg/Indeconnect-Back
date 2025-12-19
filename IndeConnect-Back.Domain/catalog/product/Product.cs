using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain.catalog.product;

public class Product
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!; // "T-shirt Streetwear - Rouge"
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;

    // Nouveau : appartient à un groupe
    public long? ProductGroupId { get; private set; }
    public ProductGroup? ProductGroup { get; private set; }

    // Nouveau : couleur principale de ce produit
    public long? PrimaryColorId { get; private set; }
    public Color? PrimaryColor { get; private set; }

    public long? SaleId { get; private set; }
    public Sale? Sale { get; private set; }

    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    public long CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;
    
    // Les variants deviennent juste les tailles pour CE produit
    private readonly List<ProductVariant> _variants = new();
    public IReadOnlyCollection<ProductVariant> Variants => _variants;

    private readonly List<ProductMedia> _media = new();
    public IReadOnlyCollection<ProductMedia> Media => _media;

    private readonly List<ProductKeyword> _keywords = new();
    public IReadOnlyCollection<ProductKeyword> Keywords => _keywords;

    private readonly List<ProductDetail> _details = new();
    public IReadOnlyCollection<ProductDetail> Details => _details;

    private readonly List<ProductReview> _reviews = new();
    public IReadOnlyCollection<ProductReview> Reviews => _reviews;
    
    private Product() { }

    public Product(string name, string description, decimal price, long brandId, long categoryId, long? productGroupId = null, long? primaryColorId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description required", nameof(description));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be positive");

        Name = name.Trim();
        Description = description.Trim();
        Price = price;
        BrandId = brandId;
        CategoryId = categoryId;
        ProductGroupId = productGroupId;
        PrimaryColorId = primaryColorId;

        CreatedAt = DateTimeOffset.UtcNow;
        IsEnabled = true;
        Status = ProductStatus.Draft;
    }
    
    public void UpdateInfo(
        string name,
        string description,
        decimal price,
        long categoryId,
        long? primaryColorId,
        ProductStatus status
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description required", nameof(description));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be positive");

        Name = name.Trim();
        Description = description.Trim();
        Price = price;
        CategoryId = categoryId;
        PrimaryColorId = primaryColorId;
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Calcule le prix actuel en tenant compte d'une promotion active
    /// </summary>
    /// <param name="at">Date à laquelle vérifier (par défaut: maintenant)</param>
    /// <returns>Prix avec réduction si promotion active, sinon prix de base</returns>
    public decimal CalculateCurrentPrice(DateTimeOffset? at = null)
    {
        var checkDate = at ?? DateTimeOffset.UtcNow;

        if (Sale == null || !Sale.IsActive
            || Sale.StartDate > checkDate
            || Sale.EndDate < checkDate)
        {
            return Price;
        }

        return Price * (1 - Sale.DiscountPercentage / 100);
    }

    /// <summary>
    /// Calcule le stock total de toutes les variantes
    /// </summary>
    public int GetTotalStock()
    {
        return Variants.Sum(v => v.StockCount);
    }

    /// <summary>
    /// Vérifie si le produit est disponible à l'achat
    /// </summary>
    public bool IsAvailableForPurchase()
    {
        return IsEnabled
            && Status == ProductStatus.Online
            && GetTotalStock() > 0;
    }

    /// <summary>
    /// Calcule la note moyenne des reviews approuvées
    /// </summary>
    public double GetAverageRating()
    {
        var approvedReviews = Reviews.Where(r => r.Status == ReviewStatus.Approved).ToList();
        return approvedReviews.Any() ? approvedReviews.Average(r => (double)r.Rating) : 0.0;
    }

    /// <summary>
    /// Compte le nombre de reviews approuvées
    /// </summary>
    public int GetApprovedReviewsCount()
    {
        return Reviews.Count(r => r.Status == ReviewStatus.Approved);
    }

    /// <summary>
    /// Obtient l'image principale du produit
    /// </summary>
    public ProductMedia? GetPrimaryImage()
    {
        return Media.FirstOrDefault(m => m.IsPrimary)
            ?? Media.OrderBy(m => m.DisplayOrder).FirstOrDefault();
    }

    /// <summary>
    /// Obtient l'URL de l'image principale du produit
    /// </summary>
    public string? GetPrimaryImageUrl()
    {
        return GetPrimaryImage()?.Url;
    }
}