namespace IndeConnect_Back.Application.DTOs.Orders;

public class OrderItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public long? VariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}