using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Domain.payment;
/**
 * Represents User's payment method.
 */
public class UserPaymentMethod
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public User User { get; private set; } = default!;
    public long PaymentProviderId { get; private set; }
    public PaymentProvider PaymentProvider { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    private UserPaymentMethod() { }
    public UserPaymentMethod(long userId, long providerId)
    {
        UserId = userId;
        PaymentProviderId = providerId;
        IsActive = true;
    }
}