using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain.catalog.product;

/**
 * Represents a Brand's product with variant management (size + color combinations)
 * Stock is managed at the variant level, not globally
 */

public class Product
{
    // General information
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;

    // Sale (promotion)
    public long? SaleId { get; private set; }
    public Sale? Sale { get; private set; }

    // Brand
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    // Category
    public long CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;

    // MetaDatas
    private readonly List<ProductMedia> _media = new();
    public IReadOnlyCollection<ProductMedia> Media => _media;
    
    // Variants
    private readonly List<ProductVariant> _variants = new();
    public IReadOnlyCollection<ProductVariant> Variants => _variants;

    private readonly List<ProductKeyword> _keywords = new();
    public IReadOnlyCollection<ProductKeyword> Keywords => _keywords;

    private readonly List<ProductDetail> _details = new();
    public IReadOnlyCollection<ProductDetail> Details => _details;

    // User's Reviews
    private readonly List<ProductReview> _reviews = new();
    public IReadOnlyCollection<ProductReview> Reviews => _reviews;
    
    private Product() { }

    public Product(string name, string description, decimal price, long brandId, long categoryId)
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

        CreatedAt = DateTimeOffset.UtcNow;
        IsEnabled = true;
        Status = ProductStatus.Draft;
    }
}
