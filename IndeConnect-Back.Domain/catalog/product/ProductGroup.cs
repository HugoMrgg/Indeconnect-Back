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

    /// <summary>
    /// Obtient tous les produits en ligne du groupe
    /// </summary>
    public IEnumerable<Product> GetOnlineProducts()
    {
        return Products.Where(p => p.IsEnabled && p.Status == ProductStatus.Online);
    }

    /// <summary>
    /// Obtient le nombre de couleurs (produits) disponibles dans le groupe
    /// </summary>
    public int GetAvailableColorsCount()
    {
        return GetOnlineProducts().Count();
    }

    /// <summary>
    /// Vérifie si le groupe a au moins un produit
    /// </summary>
    public bool HasProducts()
    {
        return Products.Any();
    }

    /// <summary>
    /// Vérifie si le groupe a au moins un produit disponible à l'achat
    /// </summary>
    public bool HasAvailableProducts()
    {
        return GetOnlineProducts().Any(p => p.IsAvailableForPurchase());
    }

    /// <summary>
    /// Met à jour les informations de base du groupe
    /// </summary>
    public void UpdateInfo(string name, string baseDescription, long categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required", nameof(name));
        if (string.IsNullOrWhiteSpace(baseDescription))
            throw new ArgumentException("Description required", nameof(baseDescription));

        Name = name.Trim();
        BaseDescription = baseDescription.Trim();
        CategoryId = categoryId;
    }
}