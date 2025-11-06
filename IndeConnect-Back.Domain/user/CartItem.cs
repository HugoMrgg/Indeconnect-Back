namespace IndeConnect_Back.Domain;

public class CartItem
{
    public long CartId { get; private set; }
    public Cart Cart { get; private set; } = default!;

    public long ProductId { get; private set; }
    public Product Product { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private CartItem(Product product)
    {
        Product = product;
    } // EF

    public CartItem(long cartId, long productId, int quantity, decimal unitPrice, Product product)
    {
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Product = product;
    }

    public void Increase(int delta) => Quantity += delta;
    public void SetQuantity(int qty)
    {
        if (qty < 0) throw new ArgumentOutOfRangeException(nameof(qty));
        Quantity = qty;
    }
}