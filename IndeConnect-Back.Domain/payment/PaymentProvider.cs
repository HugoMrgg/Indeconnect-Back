namespace IndeConnect_Back.Domain.payment;
/**
 * Represents a Payement method proposed by Indeconnect
 */
public class PaymentProvider
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!; 
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public string? LogoUrl { get; private set; } 
    
    private PaymentProvider() { }
    
    public PaymentProvider(string name, string? description = null, string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
            
        Name = name.Trim();
        Description = description?.Trim();
        LogoUrl = logoUrl?.Trim();
        IsEnabled = true;
    }
}
