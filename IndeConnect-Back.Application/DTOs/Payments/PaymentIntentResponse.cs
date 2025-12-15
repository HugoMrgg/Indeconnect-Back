namespace IndeConnect_Back.Application.DTOs.Payments;

public class PaymentIntentResponse
{
    public string ClientSecret { get; set; } = string.Empty;
    public long OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
}