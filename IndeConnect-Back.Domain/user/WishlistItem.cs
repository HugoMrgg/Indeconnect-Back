namespace IndeConnect_Back.Domain;

public class WishlistItem
{
    public long WishlistId { get; set; }
    public Wishlist Wishlist { get; set; }
    public long ProductId { get; set; }
    public DateTime AddedAt { get; set; }
}