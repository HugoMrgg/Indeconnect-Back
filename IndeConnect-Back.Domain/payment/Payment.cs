using IndeConnect_Back.Domain.order;

namespace IndeConnect_Back.Domain.payment;

public class Payment
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public Order Order { get; private set; } = default!;

    public long PaymentProviderId { get; private set; }
    public PaymentProvider PaymentProvider { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

    public string ProviderPaymentId { get; private set; } = default!; // Ex : ID Stripe/PayPal
    public string? Currency { get; private set; } // Facile à étendre pour multi-devise
    public decimal Amount { get; private set; }

    public string? RawPayload { get; private set; } // Pour audit/debug éventuel (non PII !)

    private Payment() { }
    public Payment(long orderId, long paymentProviderId, decimal amount, string providerPaymentId, string? currency)
    {
        OrderId = orderId;
        PaymentProviderId = paymentProviderId;
        CreatedAt = DateTime.UtcNow;
        Amount = amount;
        Currency = currency ?? "EUR";
        ProviderPaymentId = providerPaymentId;
    }

    public void MarkPaid()
    {
        Status = PaymentStatus.Paid;
    }

    public void MarkFailed()
    {
        Status = PaymentStatus.Failed;
    }

    public void MarkRefunded()
    {
        Status = PaymentStatus.Refunded;
    }
}

