using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IOrderEmailTemplateService
{
    string GenerateOrderConfirmationEmail(Order order, User user, ShippingAddress address);
    string GeneratePaymentConfirmationEmail(Order order, User user);
    string GenerateOrderProcessingEmail(Order order, User user);

    // New BrandDelivery methods
    string GenerateOrderShippedEmail(Order order, User user, BrandDelivery brandDelivery);
    string GenerateOrderInTransitEmail(Order order, User user, BrandDelivery brandDelivery);
    string GenerateOrderOutForDeliveryEmail(Order order, User user, BrandDelivery brandDelivery);
    string GenerateOrderDeliveredEmail(Order order, User user, BrandDelivery brandDelivery);
}
