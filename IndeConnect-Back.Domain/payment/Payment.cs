using IndeConnect_Back.Domain.order;

namespace IndeConnect_Back.Domain.payment;
/**
 * Represents a User's payment associated to an order.
 */
public class Payment
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;
    
    public long PaymentProviderId { get; private set; }
    public PaymentProvider PaymentProvider { get; private set; } = default!;
    
    public DateTimeOffset CreatedAt { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string Currency { get; private set; } = "EUR";
    public decimal Amount { get; private set; }

    public string? TransactionId { get; private set; } 
    public string? RawPayload { get; private set; }

    private Payment() { }
    
    public Payment(long orderId, long paymentProviderId, decimal amount, string? currency = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
            
        OrderId = orderId;
        PaymentProviderId = paymentProviderId;
        Amount = amount;
        Currency = currency ?? "EUR";
        CreatedAt = DateTimeOffset.UtcNow;
        Status = PaymentStatus.Pending;
    }
    
    public void MarkAsPaid(string transactionId, string? rawPayload = null)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID is required", nameof(transactionId));
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be marked as paid");
            
        TransactionId = transactionId;
        RawPayload = rawPayload;
        Status = PaymentStatus.Paid;
    }
    
    public void MarkAsFailed(string? rawPayload = null)
    {
        RawPayload = rawPayload;
        Status = PaymentStatus.Failed;
    }
    
    public void Refund(string? rawPayload = null)
    {
        if (Status != PaymentStatus.Paid)
            throw new InvalidOperationException("Only paid payments can be refunded");
            
        RawPayload = rawPayload;
        Status = PaymentStatus.Refunded;
    }
}


