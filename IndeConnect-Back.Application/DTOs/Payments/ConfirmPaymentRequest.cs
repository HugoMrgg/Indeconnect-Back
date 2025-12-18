namespace IndeConnect_Back.Application.DTOs.Payments;


public class ConfirmPaymentRequest
{
    public long OrderId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
}