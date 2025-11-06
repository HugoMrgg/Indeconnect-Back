namespace IndeConnect_Back.Domain.catalog.product;

public class ProductMedia
{
    public long Id { get; private set; }
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    
    public string Url { get; private set; } = default!;
    public MediaType Type { get; private set; } = MediaType.Image;
    public int DisplayOrder { get; private set; }
    public string? AltText { get; private set; }
    
    private ProductMedia() { }
    
    public ProductMedia(long productId, string url, MediaType type, int displayOrder = 0, string? altText = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is required", nameof(url));
            
        ProductId = productId;
        Url = url.Trim();
        Type = type;
        DisplayOrder = displayOrder;
        AltText = altText?.Trim();
    }
}
