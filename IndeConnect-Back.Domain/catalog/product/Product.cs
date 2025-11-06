using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Domain;

public class Product
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;

    public long? SaleId { get; private set; }
    public Sale? Sale { get; private set; }

    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    public long CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;

    private readonly List<ProductMedia> _media = new();
    public IReadOnlyCollection<ProductMedia> Media => _media;
    public void AddMedia(ProductMedia media) => _media.Add(media);
    
    // nav: M2M via entités de jointure (certaines avec payload)
    private readonly List<ProductSize> _sizes = new();
    public IReadOnlyCollection<ProductSize> Sizes => _sizes;

    private readonly List<ProductColor> _colors = new();
    public IReadOnlyCollection<ProductColor> Colors => _colors;

    private readonly List<ProductKeyword> _keywords = new();
    public IReadOnlyCollection<ProductKeyword> Keywords => _keywords;

    private readonly List<ProductDetail> _details = new();
    public IReadOnlyCollection<ProductDetail> Details => _details;

    private readonly List<ProductReview> _reviews = new();
    public IReadOnlyCollection<ProductReview> Reviews => _reviews;
    public void AddReview(ProductReview review) => _reviews.Add(review);
    
    private Product() { }

    public Product(string name, string description, decimal price, long brandId, long categoryId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required", nameof(name));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description required", nameof(description));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));

        Name = name.Trim();
        Description = description.Trim();
        Price = price;
        BrandId = brandId;
        CategoryId = categoryId;

        CreatedAt = DateTime.UtcNow;
        IsEnabled = true;
        Status = ProductStatus.Draft;
    }

    public void Publish()
    {
        if (!IsEnabled) throw new InvalidOperationException("Disabled product");
        Status = ProductStatus.Online;
    }

    public void Disable() => Status = ProductStatus.Disabled;
    public void Enable()  { IsEnabled = true; if (Status == ProductStatus.Disabled) Status = ProductStatus.Draft; }
}
