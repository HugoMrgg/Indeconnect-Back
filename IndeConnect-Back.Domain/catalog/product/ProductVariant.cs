namespace IndeConnect_Back.Domain.catalog.product;

public class ProductVariant
{
    public long Id { get; private set; }
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public long? SizeId { get; private set; }
    public Size? Size { get; private set; }

    public string SKU { get; private set; } = default!;
    public int StockCount { get; private set; }
    public decimal? PriceOverride { get; private set; }

    private ProductVariant() { }

    public ProductVariant(long productId, string sku, int stock, long? sizeId = null, decimal? priceOverride = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required", nameof(sku));
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(stock));

        ProductId = productId;
        SKU = sku.Trim().ToUpper();
        StockCount = stock;
        SizeId = sizeId;
        PriceOverride = priceOverride;
    }
}