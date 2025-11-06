namespace IndeConnect_Back.Domain;

public class ProductDetail
{
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public long DetailId { get; private set; }
    public Detail Detail { get; private set; } = default!;

    private ProductDetail() { }
    public ProductDetail(long productId, long detailId) { ProductId = productId; DetailId = detailId; }
}