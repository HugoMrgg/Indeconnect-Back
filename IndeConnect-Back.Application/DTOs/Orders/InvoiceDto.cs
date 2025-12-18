namespace IndeConnect_Back.Application.DTOs.Orders;

public class InvoiceDto
{
    public long Id { get; set; }
    public long BrandId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
}