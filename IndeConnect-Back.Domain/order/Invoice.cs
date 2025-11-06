using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain.order;

public class Invoice
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;
    
    public DateTime IssuedAt { get; private set; }
    public string InvoiceNumber { get; private set; } = default!;
    public decimal Amount { get; private set; }

    private Invoice() { }
    public Invoice(long orderId, long brandId, string invoiceNumber, decimal amount)
    {
        OrderId = orderId;
        BrandId = brandId;
        InvoiceNumber = invoiceNumber.Trim();
        Amount = amount;
        IssuedAt = DateTime.UtcNow;
    }
}
