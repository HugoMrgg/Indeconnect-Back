using IndeConnect_Back.Domain.payment;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentIntentAsync(long orderId);
    Task<Payment> ConfirmPaymentAsync(long orderId, string paymentIntentId);
}