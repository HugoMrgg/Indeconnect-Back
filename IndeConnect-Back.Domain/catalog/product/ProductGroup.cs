using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain.catalog.product;

public class ProductGroup
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!; // "T-shirt Streetwear"
    public string BaseDescription { get; private set; } = default!;
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;
    public long CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;
    
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products;

    private ProductGroup() { }

    public ProductGroup(string name, string baseDescription, long brandId, long categoryId)
    {
        Name = name.Trim();
        BaseDescription = baseDescription.Trim();
        BrandId = brandId;
        CategoryId = categoryId;
    }
}