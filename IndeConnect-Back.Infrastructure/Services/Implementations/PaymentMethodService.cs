using IndeConnect_Back.Application.DTOs.Payments;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
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
        
        var stripeKey = Environment.GetEnvironmentVariable("STRIPE_API_SECRET");

        if (string.IsNullOrEmpty(stripeKey))
        {
            _logger.LogError("STRIPE_API_SECRET is not configured");
            throw new InvalidOperationException("Stripe API key is missing");
        }

        _logger.LogInformation("Stripe configured with key: {KeyPrefix}...", stripeKey.Substring(0, 12));
        StripeConfiguration.ApiKey = stripeKey;
    }

    private async Task<string> GetOrCreateStripeCustomerIdAsync(long? userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new InvalidOperationException("User not found");

        if (!string.IsNullOrWhiteSpace(user.StripeCustomerId))
            return user.StripeCustomerId;

        // Create Stripe customer if missing
        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = user.Email,
            Metadata = new Dictionary<string, string?>
            {
                { "user_id", userId.ToString() }
            }
        });

        user.SetStripeCustomerId(customer.Id);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Stripe Customer created: {CustomerId} for User {UserId}", customer.Id, userId);
        return customer.Id;
    }

    private static string? ExtractDefaultPaymentMethodId(Customer customer)
    {
        var dpm = customer.InvoiceSettings?.DefaultPaymentMethod;
        if (dpm == null) return null;
        
        var idProp = dpm.GetType().GetProperty("Id");
        if (idProp?.GetValue(dpm) is string s && !string.IsNullOrWhiteSpace(s)) return s;

        var asString = dpm.ToString();
        return string.IsNullOrWhiteSpace(asString) ? null : asString;
    }
    
    public async Task<List<PaymentMethodDto>> GetUserPaymentMethodsAsync(long? userId)
    {
        var customerId = await GetOrCreateStripeCustomerIdAsync(userId);

        var customerService = new CustomerService();
        var customer = await customerService.GetAsync(customerId);
        var defaultPaymentMethodId = ExtractDefaultPaymentMethodId(customer);

        var pmService = new Stripe.PaymentMethodService();

        // 1) Cards
        var cards = await pmService.ListAsync(new PaymentMethodListOptions
        {
            Customer = customerId,
            Type = "card"
        });

        // 2) PayPal (OPTIONNEL)
        // Si tu ne veux pas gérer PayPal dans le wallet, tu peux supprimer ce bloc et rester "card only".
        StripeList<PaymentMethod>? paypals = null;
        try
        {
            paypals = await pmService.ListAsync(new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "paypal"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to list PayPal payment methods for customer {CustomerId}", customerId);
        }

        var all = new List<PaymentMethod>();
        all.AddRange(cards.Data);

        if (paypals?.Data != null)
            all.AddRange(paypals.Data);

        return all.Select(pm => new PaymentMethodDto
        {
            Id = pm.Id,
            Type = pm.Type, // "card" ou "paypal"
            Brand = pm.Type == "card" ? (pm.Card?.Brand ?? "card") : "paypal",
            Last4 = pm.Type == "card" ? (pm.Card?.Last4 ?? "****") : "----",
            ExpiryMonth = pm.Type == "card" ? (pm.Card?.ExpMonth ?? 0) : 0,
            ExpiryYear = pm.Type == "card" ? (pm.Card?.ExpYear ?? 0) : 0,
            IsDefault = pm.Id == defaultPaymentMethodId
        }).ToList();
    }

    public async Task DeletePaymentMethodAsync(long? userId, string paymentMethodId)
    {
        var customerId = await GetOrCreateStripeCustomerIdAsync(userId);

        var pmService = new Stripe.PaymentMethodService();
        var customerService = new CustomerService();

        // 1) Check ownership
        var pm = await pmService.GetAsync(paymentMethodId);
        if (!string.Equals(pm.CustomerId, customerId, StringComparison.Ordinal))
            throw new InvalidOperationException("Payment method does not belong to this user");

        // 2) Load customer + default
        var customer = await customerService.GetAsync(customerId);
        var defaultPmId = ExtractDefaultPaymentMethodId(customer);

        // 3) If this is default, pick another one (or clear)
        if (defaultPmId == paymentMethodId)
        {
            // Find another payment method (card first)
            var cards = await pmService.ListAsync(new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card"
            });

            var replacement = cards.Data.FirstOrDefault(x => x.Id != paymentMethodId);

            // Optional: try paypal as fallback too
            if (replacement == null)
            {
                try
                {
                    var paypals = await pmService.ListAsync(new PaymentMethodListOptions
                    {
                        Customer = customerId,
                        Type = "paypal"
                    });
                    replacement = paypals.Data.FirstOrDefault(x => x.Id != paymentMethodId);
                }
                catch { /* ignore if not supported */ }
            }

            await customerService.UpdateAsync(customerId, new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = replacement?.Id // null => clear default
                }
            });

            _logger.LogInformation("Default payment method changed from {Old} to {New} for customer {CustomerId}",
                paymentMethodId, replacement?.Id ?? "(none)", customerId);
        }

        // 4) Detach
        await pmService.DetachAsync(paymentMethodId);

        _logger.LogInformation("Payment method {PaymentMethodId} detached from customer {CustomerId}",
            paymentMethodId, customerId);
    }

    public async Task<PaymentMethodDto> SetDefaultPaymentMethodAsync(long? userId, string paymentMethodId)
    {
        var customerId = await GetOrCreateStripeCustomerIdAsync(userId);

        var pmService = new Stripe.PaymentMethodService();
        var pm = await pmService.GetAsync(paymentMethodId);

        // Sécurité: vérifier que la méthode appartient au customer
        if (!string.Equals(pm.CustomerId, customerId, StringComparison.Ordinal))
            throw new InvalidOperationException("Payment method does not belong to this customer");

        var customerService = new CustomerService();
        await customerService.UpdateAsync(customerId, new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = paymentMethodId
            }
        });

        await customerService.UpdateAsync(customerId, new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = paymentMethodId
            }
        });

        _logger.LogInformation("Payment Method {PaymentMethodId} set as default for Customer {CustomerId}",
            paymentMethodId, customerId);
        
        // 🔥 re-read
        var updatedCustomer = await customerService.GetAsync(customerId);
        var defaultId = ExtractDefaultPaymentMethodId(updatedCustomer);

        return new PaymentMethodDto
        {
            Id = pm.Id,
            Type = pm.Type,
            Brand = pm.Card?.Brand ?? "card",
            Last4 = pm.Card?.Last4 ?? "****",
            ExpiryMonth = pm.Card?.ExpMonth ?? 0,
            ExpiryYear = pm.Card?.ExpYear ?? 0,
            IsDefault = pm.Id == defaultId
        };
    }
    
    public async Task<string> CreateSetupIntentAsync(long? userId)
    {
        var customerId = await GetOrCreateStripeCustomerIdAsync(userId);

        var service = new SetupIntentService();
        var setupIntent = await service.CreateAsync(new SetupIntentCreateOptions
        {
            Customer = customerId,

            // ✅ Laisse Stripe gérer les méthodes via Dashboard (comme tu veux partout)
            AutomaticPaymentMethods = new SetupIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },

            Usage = "off_session",
            Metadata = new Dictionary<string, string?>
            {
                { "user_id", userId.ToString() }
            }
        });

        return setupIntent.ClientSecret;
    }
}
