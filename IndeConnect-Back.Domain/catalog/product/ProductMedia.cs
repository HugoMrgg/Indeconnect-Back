namespace IndeConnect_Back.Domain.catalog.product;

public class ProductMedia
{
    public long Id { get; private set; }
    public long ProductId { get; private set; }
    public Product Product { get; private set; }
    public string Url { get; private set; } = default!;
    public MediaType Type { get; private set; } = MediaType.Image;
    public int DisplayOrder { get; private set; }
    public string? AltText { get; private set; }
    private ProductMedia() { }
    public ProductMedia(long productId, string url, MediaType type)
    {
        ProductId = productId;
        Url = url;
        Type = type;
    }
}
