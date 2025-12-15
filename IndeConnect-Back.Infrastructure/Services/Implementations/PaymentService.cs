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

    public PaymentService(AppDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;

        var stripeKey = Environment.GetEnvironmentVariable("STRIPE_API_SECRET");

        if (string.IsNullOrEmpty(stripeKey))
        {
            _logger.LogError("⚠STRIPE_API_SECRET is not configured!");
            throw new InvalidOperationException("Stripe API key is missing");
        }

        _logger.LogInformation("Stripe configured with key: {KeyPrefix}...", stripeKey.Substring(0, 12));
        StripeConfiguration.ApiKey = stripeKey;
    }

    private async Task<string> GetOrCreateStripeCustomerAsync(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");

        // Si déjà créé, le retourner
        if (!string.IsNullOrEmpty(user.StripeCustomerId))
        {
            _logger.LogInformation("Reusing existing Stripe Customer {CustomerId} for User {UserId}", 
                user.StripeCustomerId, userId);
            return user.StripeCustomerId;
        }

        // Créer un nouveau customer Stripe
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
            _logger.LogWarning("⚠Order {OrderId} not found", orderId);
            throw new InvalidOperationException("Commande introuvable");
        }

        _logger.LogInformation("Order {OrderId} found - Total: {Total} {Currency}", 
            orderId, order.TotalAmount, order.Currency);

        // Créer ou récupérer le Stripe Customer
        var customerId = await GetOrCreateStripeCustomerAsync(order.UserId);

        // Vérifier si un Payment existe déjà
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Pending);

        if (existingPayment != null && !string.IsNullOrEmpty(existingPayment.TransactionId))
        {
            _logger.LogInformation("♻️ Reusing existing PaymentIntent {PaymentIntentId}", 
                existingPayment.TransactionId);

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(existingPayment.TransactionId);
            
            return paymentIntent.ClientSecret;
        }

        // Récupérer le PaymentProvider "Stripe"
        var stripeProvider = await _context.PaymentProviders
            .FirstOrDefaultAsync(p => p.Name == "Stripe");

        if (stripeProvider == null)
        {
            _logger.LogError("Stripe provider not found in database");
            throw new InvalidOperationException("Stripe provider not configured");
        }

        // Créer PaymentIntent avec support PayPal + Customer
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(order.TotalAmount * 100),
            Currency = order.Currency.ToLower(),
            Customer = customerId, 
            
            PaymentMethodTypes = new List<string>
            {
                "card",        // Cartes bancaires
                "paypal",      // PayPal
                "bancontact"   // Bancontact (déjà utilisé)
            },
            
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
            throw new InvalidOperationException($"Erreur Stripe: {ex.Message}");
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
            throw new InvalidOperationException("Paiement introuvable");
        }

        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            _logger.LogInformation("Stripe PaymentIntent status: {Status}", paymentIntent.Status);

            if (paymentIntent.Status == "succeeded")
            {
                payment.MarkAsPaid(paymentIntentId);
                
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Paid;
                    _logger.LogInformation("Order {OrderId} marked as Paid", orderId);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment confirmed for Order {OrderId}", orderId);
            }
            else
            {
                _logger.LogWarning("PaymentIntent {PaymentIntentId} has status {Status}", 
                    paymentIntentId, paymentIntent.Status);
                throw new InvalidOperationException($"Le paiement n'a pas abouti (statut: {paymentIntent.Status})");
            }

            return payment;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error confirming payment");
            throw new InvalidOperationException($"Erreur Stripe: {ex.Message}");
        }
    }
}