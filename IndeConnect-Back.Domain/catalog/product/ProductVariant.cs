namespace IndeConnect_Back.Domain.catalog.product;

public class ProductVariant
{
    public long Id { get; private set; }
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    
    public string SKU { get; private set; } = default!;
    public long? SizeId { get; private set; }
    public Size? Size { get; private set; }
    
    public int StockCount { get; private set; }
    public decimal? PriceOverride { get; private set; }

    private ProductVariant() { }
    
    public ProductVariant(long productId, string sku, int stockCount = 0, long? sizeId = null, decimal? priceOverride = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required", nameof(sku));
            
        ProductId = productId;
        SKU = sku.Trim().ToUpper();
        StockCount = stockCount;
        SizeId = sizeId;
        PriceOverride = priceOverride;
    }
    
    public void DecrementStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        
        if (StockCount < quantity)
            throw new InvalidOperationException($"Stock insuffisant. Disponible: {StockCount}, Demandé: {quantity}");
            
        StockCount -= quantity;
    }

    public void IncrementStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
        StockCount += quantity;
    }
    
    public void SetStock(int newStock)
    {
        if (newStock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(newStock));
            
        StockCount = newStock;
    } 
    public void UpdateStock(int newStock)
    {
        if (newStock < 0)
            throw new ArgumentException("Stock cannot be negative");
    
        StockCount = newStock;
    }
}