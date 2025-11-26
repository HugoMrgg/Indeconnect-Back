using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Domain.user;

/**
 * Represents an item of a Cart, an Item has a Product, a ProductVariant (size), a quantity and a unitPrice
 */
public class CartItem
{
    // Composite key : CartId + ProductVariantId (pas ProductId, car on veut suivre la taille exacte)
    public long CartId { get; private set; }
    public Cart Cart { get; private set; } = default!;

    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    
    // NOUVEAU : Référence à la variante (taille) choisie
    public long ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = default!;
    
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    private CartItem() { }

    public CartItem(long cartId, long productId, long productVariantId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        CartId   = cartId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        AddedAt  = DateTimeOffset.UtcNow;
    }

    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity += quantity;
    }

    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity += quantity;
    }

    public void SetQuantity(int newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(newQuantity));
        Quantity = newQuantity;
    }
}