namespace IndeConnect_Back.Domain.catalog.product;

public class ProductMedia
{
    public long Id { get; private set; }
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    
    public string Url { get; private set; } = default!;
    public MediaType Type { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }

    private ProductMedia() { }

    public ProductMedia(long productId, string url, MediaType type, int displayOrder, bool isPrimary = false)
    {
        ProductId = productId;
        Url = url;
        Type = type;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
    }
}