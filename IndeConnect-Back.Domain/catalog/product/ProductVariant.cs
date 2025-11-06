namespace IndeConnect_Back.Domain.catalog.product;
/**
 * Represents a product variant with its own stock management
 * Each variant is a unique combination of size + color with independent stock
 */
public class ProductVariant
{
    public long Id { get; private set; }
    
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    // Size
    public long? SizeId { get; private set; }
    public Size? Size { get; private set; }

    // Color 
    public long? ColorId { get; private set; }
    public Color? Color { get; private set; }

    public string SKU { get; private set; } = default!;
    
    public int StockCount { get; private set; }
    
    public decimal? PriceOverride { get; private set; }

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
