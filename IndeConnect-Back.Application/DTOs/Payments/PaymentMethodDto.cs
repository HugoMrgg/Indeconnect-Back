namespace IndeConnect_Back.Application.DTOs.Payments;

public class PaymentMethodDto
{
    public string Id { get; set; } = default!;
    public string Type { get; set; } = "card"; // "card" ou "paypal"
    public string Brand { get; set; } = default!; // "visa", "mastercard", "paypal"
    public string Last4 { get; set; } = default!;
    public long ExpiryMonth { get; set; }
    public long ExpiryYear { get; set; }
    public bool IsDefault { get; set; }
}