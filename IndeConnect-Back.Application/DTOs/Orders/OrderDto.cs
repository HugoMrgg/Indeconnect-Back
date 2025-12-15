namespace IndeConnect_Back.Application.DTOs.Orders;

public class OrderDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long ShippingAddressId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTimeOffset PlacedAt { get; set; }
    public string Currency { get; set; } = "EUR";
        
    public List<OrderItemDto> Items { get; set; } = new();
    public List<InvoiceDto> Invoices { get; set; } = new();
}