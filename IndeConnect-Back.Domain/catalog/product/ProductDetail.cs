namespace IndeConnect_Back.Domain.catalog.product;

public class ProductDetail
{
    public long Id { get; private set; }
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string Key { get; private set; } = default!; 
    public string Value { get; private set; } = default!;
    public int DisplayOrder { get; private set; }
    
    private ProductDetail() { }
    
    public ProductDetail(long productId, string key, string value, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key is required", nameof(key));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is required", nameof(value));
            
        ProductId = productId;
        Key = key.Trim();
        Value = value.Trim();
        DisplayOrder = displayOrder;
    }
}
