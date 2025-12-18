using IndeConnect_Back.Application.DTOs.Payments;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Stripe;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentMethodService> _logger;

    public PaymentMethodService(AppDbContext context, ILogger<PaymentMethodService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private async Task<string> GetStripeCustomerIdAsync(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.StripeCustomerId))
            throw new InvalidOperationException("User has no Stripe Customer ID");

        return user.StripeCustomerId;
    }

    public async Task<List<PaymentMethodDto>> GetUserPaymentMethodsAsync(long userId)
    {
        var customerId = await GetStripeCustomerIdAsync(userId);

        var service = new Stripe.PaymentMethodService();
        var paymentMethods = await service.ListAsync(new PaymentMethodListOptions
        {
            Customer = customerId,
            Type = "card"
        });

        var customerService = new CustomerService();
        var customer = await customerService.GetAsync(customerId);
        var defaultPaymentMethodId = customer.InvoiceSettings?.DefaultPaymentMethod?.Id;

        return paymentMethods.Data.Select(pm => new PaymentMethodDto
        {
            Id = pm.Id,
            Type = pm.Type, // "card" ou "paypal"
            Brand = pm.Card?.Brand ?? "paypal",
            Last4 = pm.Card?.Last4 ?? "****",
            ExpiryMonth = pm.Card?.ExpMonth ?? 0,
            ExpiryYear = pm.Card?.ExpYear ?? 0,
            IsDefault = pm.Id == defaultPaymentMethodId
        }).ToList();
    }

    public async Task DeletePaymentMethodAsync(long userId, string paymentMethodId)
    {
        var customerId = await GetStripeCustomerIdAsync(userId);

        var service = new Stripe.PaymentMethodService();
        await service.DetachAsync(paymentMethodId);

        _logger.LogInformation("Payment Method {PaymentMethodId} detached from Customer {CustomerId}", 
            paymentMethodId, customerId);
    }

    public async Task<PaymentMethodDto> SetDefaultPaymentMethodAsync(long userId, string paymentMethodId)
    {
        var customerId = await GetStripeCustomerIdAsync(userId);

        var customerService = new CustomerService();
        await customerService.UpdateAsync(customerId, new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = paymentMethodId
            }
        });

        _logger.LogInformation("Payment Method {PaymentMethodId} set as default", paymentMethodId);

        var pmService = new Stripe.PaymentMethodService();
        var pm = await pmService.GetAsync(paymentMethodId);

        return new PaymentMethodDto
        {
            Id = pm.Id,
            Type = pm.Type,
            Brand = pm.Card?.Brand ?? "paypal",
            Last4 = pm.Card?.Last4 ?? "****",
            ExpiryMonth = pm.Card?.ExpMonth ?? 0,
            ExpiryYear = pm.Card?.ExpYear ?? 0,
            IsDefault = true
        };
    }
}
