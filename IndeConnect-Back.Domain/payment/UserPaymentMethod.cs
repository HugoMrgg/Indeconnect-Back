namespace IndeConnect_Back.Domain.payment;

public class UserPaymentMethod
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public User User { get; private set; } = default!;
    public long PaymentProviderId { get; private set; }
    public PaymentProvider PaymentProvider { get; private set; } = default!;
    public string Reference { get; private set; } = default!; // ex: Stripe customer ID, PayPalToken
    public bool IsActive { get; private set; } = true;
    private UserPaymentMethod() { }
    public UserPaymentMethod(long userId, long providerId, string reference)
    {
        UserId = userId;
        PaymentProviderId = providerId;
        Reference = reference;
        IsActive = true;
    }
}