namespace IndeConnect_Back.Domain.catalog.product;

public class ProductVariant
{
    public long Id { get; private set; }
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public long? SizeId { get; private set; }
    public Size? Size { get; private set; }

    public long? ColorId { get; private set; }
    public Color? Color { get; private set; }

    public string SKU { get; private set; } = default!;
    public int StockCount { get; private set; }
    public decimal? PriceOverride { get; private set; }

    // ✅ Images spécifiques à cette variante
    private readonly List<ProductVariantMedia> _media = new();
    public IReadOnlyCollection<ProductVariantMedia> Media => _media;

    private ProductVariant() { }

    public ProductVariant(long productId, string sku, int stock, long? sizeId = null, long? colorId = null, decimal? priceOverride = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required", nameof(sku));
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(stock));

        ProductId = productId;
        SKU = sku.Trim().ToUpper();
        StockCount = stock;
        SizeId = sizeId;
        ColorId = colorId;
        PriceOverride = priceOverride;
    }
}

public class ProductVariantMedia
{
    public long Id { get; private set; }
    public long VariantId { get; private set; }
    public ProductVariant Variant { get; private set; } = default!;
    
    public string Url { get; private set; } = default!;
    public MediaType Type { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }

    private ProductVariantMedia() { }

    public ProductVariantMedia(long variantId, string url, MediaType type, int displayOrder, bool isPrimary = false)
    {
        VariantId = variantId;
        Url = url;
        Type = type;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
    }
}