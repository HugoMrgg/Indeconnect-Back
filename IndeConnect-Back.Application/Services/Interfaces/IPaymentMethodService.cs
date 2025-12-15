using IndeConnect_Back.Application.DTOs.Payments;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IPaymentMethodService
{
    Task<List<PaymentMethodDto>> GetUserPaymentMethodsAsync(long userId);
    Task DeletePaymentMethodAsync(long userId, string paymentMethodId);
    Task<PaymentMethodDto> SetDefaultPaymentMethodAsync(long userId, string paymentMethodId);
}