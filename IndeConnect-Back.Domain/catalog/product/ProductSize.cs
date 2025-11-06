
namespace IndeConnect_Back.Domain;

public class ProductSize
{
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public long SizeId { get; private set; }
    public Size Size { get; private set; } = default!;

    public int StockCount { get; private set; }

    private ProductSize() { }
    public ProductSize(long productId, long sizeId, int stock)
    {
        ProductId = productId;
        SizeId = sizeId;
        StockCount = stock;
    }

    public void SetStock(int stock) => StockCount = stock;
}


