using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain.order;
/**
 * Represents an User's Invoice 
 */
public class Invoice
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;
    
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;
    
    public DateTimeOffset IssuedAt { get; private set; }
    public string InvoiceNumber { get; private set; } = default!;
    public decimal Amount { get; private set; }

    private Invoice() { }
    
    public Invoice(long orderId, long brandId, string invoiceNumber, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required", nameof(invoiceNumber));
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
            
        OrderId = orderId;
        BrandId = brandId;
        InvoiceNumber = invoiceNumber.Trim().ToUpper();
        Amount = amount;
        IssuedAt = DateTimeOffset.UtcNow;
    }
}

