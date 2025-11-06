namespace IndeConnect_Back.Domain.catalog.product;


public class ProductColor
{
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public long ColorId { get; private set; }
    public Color Color { get; private set; } = default!;

    private ProductColor() { }
    public ProductColor(long productId, long colorId) { ProductId = productId; ColorId = colorId; }
}

