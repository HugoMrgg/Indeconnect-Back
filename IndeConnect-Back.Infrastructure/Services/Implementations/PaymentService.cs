using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentService> _logger;
    private readonly IEmailService _emailService;
    private readonly IOrderEmailTemplateService _templateService;

    public PaymentService(
        AppDbContext context,
        ILogger<PaymentService> logger,
        IEmailService emailService,
        IOrderEmailTemplateService templateService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
        _templateService = templateService;

        var stripeKey = Environment.GetEnvironmentVariable("STRIPE_API_SECRET");

        if (string.IsNullOrEmpty(stripeKey))
        {
            _logger.LogError("STRIPE_API_SECRET is not configured");
            throw new InvalidOperationException("Stripe API key is missing");
        }

        _logger.LogInformation("Stripe configured with key: {KeyPrefix}...", stripeKey.Substring(0, 12));
        StripeConfiguration.ApiKey = stripeKey;
    }

    private async Task<string> GetOrCreateStripeCustomerAsync(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");

        // Return existing Stripe Customer if already created
        if (!string.IsNullOrEmpty(user.StripeCustomerId))
        {
            _logger.LogInformation("Reusing existing Stripe Customer {CustomerId} for User {UserId}",
                user.StripeCustomerId, userId);
            return user.StripeCustomerId;
        }

        // Create new Stripe Customer
        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = user.Email,
            Metadata = new Dictionary<string, string>
            {
                { "user_id", userId.ToString() }
            }
        });

        user.SetStripeCustomerId(customer.Id);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Stripe Customer created: {CustomerId} for User {UserId}", 
            customer.Id, userId);
        return customer.Id;
    }

    // CreatePaymentIntentAsync avec support PayPal + Customer
    public async Task<string> CreatePaymentIntentAsync(long orderId)
    {
        _logger.LogInformation("CreatePaymentIntentAsync called for Order {OrderId}", orderId);

        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            throw new InvalidOperationException("Order not found");
        }

        _logger.LogInformation("Order {OrderId} found - Total: {Total} {Currency}",
            orderId, order.TotalAmount, order.Currency);

        // Get or create Stripe Customer
        var customerId = await GetOrCreateStripeCustomerAsync(order.UserId);

        // Check if Payment already exists
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Pending);

        if (existingPayment != null && !string.IsNullOrEmpty(existingPayment.TransactionId))
        {
            _logger.LogInformation("Reusing existing PaymentIntent {PaymentIntentId}",
                existingPayment.TransactionId);

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(existingPayment.TransactionId);
            
            return paymentIntent.ClientSecret;
        }

        // Get Stripe PaymentProvider
        var stripeProvider = await _context.PaymentProviders
            .FirstOrDefaultAsync(p => p.Name == "Stripe");

        if (stripeProvider == null)
        {
            _logger.LogError("Stripe provider not found in database");
            throw new InvalidOperationException("Stripe provider not configured");
        }

        // C'est ici que j'ai modifié la grestion automatique de stripe pour les paiements en fonction du pays ou tu te situes
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(order.TotalAmount * 100),
            Currency = order.Currency.ToLower(),
            Customer = customerId,

            // ✅ Laisse Stripe gérer les moyens de paiement via le Dashboard
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },

            // ⚠️ SetupFutureUsage: garde-le UNIQUEMENT si tu veux réutiliser la méthode plus tard (wallet).
            // Avec certaines méthodes (bancontact/paiements redirect), ça peut être non pertinent.
            // Tu peux le laisser, mais si tu vois des comportements bizarres, mets-le à null.
            SetupFutureUsage = "off_session",

            Metadata = new Dictionary<string, string>
            {
                { "order_id", orderId.ToString() },
                { "user_id", order.UserId.ToString() }
            }
        };


        try
        {
            _logger.LogInformation("Calling Stripe API to create PaymentIntent with PayPal support...");
            
            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            _logger.LogInformation("Stripe PaymentIntent created: {PaymentIntentId} (Card + PayPal enabled)", 
                paymentIntent.Id);

            var payment = new Payment(
                orderId: orderId,
                paymentProviderId: stripeProvider.Id,
                amount: order.TotalAmount,
                transactionId: paymentIntent.Id,
                currency: order.Currency
            );

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment entity saved to DB with TransactionId: {TransactionId}", 
                paymentIntent.Id);

            return paymentIntent.ClientSecret;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating PaymentIntent");
            throw new InvalidOperationException($"Stripe error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating PaymentIntent");
            throw;
        }
    }

    public async Task<Payment> ConfirmPaymentAsync(long orderId, string paymentIntentId)
    {
        _logger.LogInformation("ConfirmPaymentAsync called for Order {OrderId}, PaymentIntent {PaymentIntentId}", 
            orderId, paymentIntentId);

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.TransactionId == paymentIntentId);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found for Order {OrderId} and PaymentIntent {PaymentIntentId}",
                orderId, paymentIntentId);
            throw new InvalidOperationException("Payment not found");
        }

        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            _logger.LogInformation("Stripe PaymentIntent status: {Status}", paymentIntent.Status);

            if (paymentIntent.Status == "succeeded")
            {
                payment.MarkAsPaid(paymentIntentId);

                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order != null)
                {
                    order.Status = OrderStatus.Paid;
                    _logger.LogInformation("Order {OrderId} marked as Paid", orderId);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment confirmed for Order {OrderId}", orderId);

                // Envoyer l'email de confirmation de paiement
                if (order != null)
                {
                    try
                    {
                        var html = _templateService.GeneratePaymentConfirmationEmail(order, order.User);
                        await _emailService.SendEmailAsync(
                            order.User.Email,
                            $"Paiement confirmé pour la commande #{orderId}",
                            html);
                        _logger.LogInformation("Payment confirmation email sent for Order {OrderId} to {Email}",
                            orderId, order.User.Email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send payment confirmation email for Order {OrderId}", orderId);
                    }
                }
            }
            else
            {
                _logger.LogWarning("PaymentIntent {PaymentIntentId} has status {Status}",
                    paymentIntentId, paymentIntent.Status);
                throw new InvalidOperationException($"Payment did not succeed (status: {paymentIntent.Status})");
            }

            return payment;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error confirming payment");
            throw new InvalidOperationException($"Stripe error: {ex.Message}");
        }
    }
}