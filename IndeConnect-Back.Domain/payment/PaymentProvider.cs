namespace IndeConnect_Back.Domain.payment;

public class PaymentProvider
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!; // "Stripe", "PayPal", etc.
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    private PaymentProvider() { }
    public PaymentProvider(string name, string? description = null)
    {
        Name = name.Trim();
        Description = description;
    }
}
