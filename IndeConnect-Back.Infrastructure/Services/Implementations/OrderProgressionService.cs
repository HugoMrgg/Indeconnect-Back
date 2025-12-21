using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

/**
 * Background service that automatically progresses orders through their lifecycle stages.
 * Simulates real-world order processing and delivery tracking without external APIs.
 */
public class OrderProgressionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderProgressionService> _logger;

    // Fixed delay for order processing (in hours) - before brand deliveries start
    private const int PaidToProcessingDelayHours = 1;

    // Percentage distribution of time between delivery stages (must sum to 100%)
    private readonly Dictionary<DeliveryStatus, double> _stagePercentages = new()
    {
        { DeliveryStatus.Preparing, 0.20 },       // 20% - Preparing the package
        { DeliveryStatus.Shipped, 0.20 },         // 20% - Initial shipping
        { DeliveryStatus.InTransit, 0.30 },       // 30% - Main transit (longest part)
        { DeliveryStatus.OutForDelivery, 0.20 },  // 20% - Out for delivery
        { DeliveryStatus.Delivered, 0.10 }        // 10% - Final delivery
    };

    public OrderProgressionService(
        IServiceProvider serviceProvider,
        ILogger<OrderProgressionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderProgressionService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing orders in background service");
            }

            // Run every 30 minutes
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }

        _logger.LogInformation("OrderProgressionService stopped");
    }

    private async Task ProcessOrdersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTimeOffset.UtcNow;

        // Get all orders that are not yet delivered or cancelled
        var ordersToProcess = await context.Orders
            .Include(o => o.BrandDeliveries)
                .ThenInclude(bd => bd.Brand)
            .Include(o => o.User)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items)
            .Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        _logger.LogInformation("Processing {Count} orders for status progression", ordersToProcess.Count);

        foreach (var order in ordersToProcess)
        {
            await ProcessOrderAsync(order, now, context, scope);
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Order processing completed");
    }

    private async Task ProcessOrderAsync(Order order, DateTimeOffset now, AppDbContext context, IServiceScope scope)
    {
        var timeSincePlacement = now - order.PlacedAt;

        switch (order.Status)
        {
            case OrderStatus.Paid:
                await HandlePaidOrderAsync(order, timeSincePlacement, context, scope);
                break;

            case OrderStatus.Processing:
                // Progress each brand delivery independently
                foreach (var brandDelivery in order.BrandDeliveries)
                {
                    await ProgressBrandDeliveryAsync(brandDelivery, order, timeSincePlacement, context, scope);
                }

                // Update global order status based on all brand deliveries
                order.UpdateGlobalStatusFromDeliveries();
                break;

            case OrderStatus.Pending:
                // Do nothing - waiting for payment
                break;
        }
    }

    private async Task HandlePaidOrderAsync(Order order, TimeSpan timeSincePlacement, AppDbContext context, IServiceScope scope)
    {
        if (timeSincePlacement.TotalHours >= PaidToProcessingDelayHours)
        {
            order.Status = OrderStatus.Processing;
            _logger.LogInformation(
                "Order {OrderId} transitioned from Paid to Processing after {Hours} hours",
                order.Id, PaidToProcessingDelayHours);

            await SendOrderProcessingEmailAsync(order, scope);
        }
    }

    /// <summary>
    /// Calculates dynamic delays for each delivery stage based on the estimated delivery time.
    /// Each stage gets a percentage of the total available time.
    /// </summary>
    private Dictionary<DeliveryStatus, double> CalculateDynamicDelays(BrandDelivery brandDelivery, Order order)
    {
        var delays = new Dictionary<DeliveryStatus, double>();

        // If no estimated delivery, use default 48h total
        var estimatedDelivery = brandDelivery.EstimatedDelivery ?? order.PlacedAt.AddHours(48);

        // Calculate total available time for delivery (excluding initial processing)
        var totalAvailableHours = (estimatedDelivery - order.PlacedAt).TotalHours - PaidToProcessingDelayHours;

        // Ensure minimum of 12 hours total for delivery stages
        if (totalAvailableHours < 12)
        {
            totalAvailableHours = 12;
        }

        // Calculate cumulative delays for each stage
        double cumulativeHours = PaidToProcessingDelayHours;

        delays[DeliveryStatus.Preparing] = cumulativeHours + (totalAvailableHours * _stagePercentages[DeliveryStatus.Preparing]);
        cumulativeHours = delays[DeliveryStatus.Preparing];

        delays[DeliveryStatus.Shipped] = cumulativeHours + (totalAvailableHours * _stagePercentages[DeliveryStatus.Shipped]);
        cumulativeHours = delays[DeliveryStatus.Shipped];

        delays[DeliveryStatus.InTransit] = cumulativeHours + (totalAvailableHours * _stagePercentages[DeliveryStatus.InTransit]);
        cumulativeHours = delays[DeliveryStatus.InTransit];

        delays[DeliveryStatus.OutForDelivery] = cumulativeHours + (totalAvailableHours * _stagePercentages[DeliveryStatus.OutForDelivery]);
        cumulativeHours = delays[DeliveryStatus.OutForDelivery];

        delays[DeliveryStatus.Delivered] = cumulativeHours + (totalAvailableHours * _stagePercentages[DeliveryStatus.Delivered]);

        return delays;
    }

    private async Task ProgressBrandDeliveryAsync(BrandDelivery brandDelivery, Order order, TimeSpan timeSincePlacement, AppDbContext context, IServiceScope scope)
    {
        // Calculate dynamic delays specific to this brand delivery based on its estimated delivery time
        var delays = CalculateDynamicDelays(brandDelivery, order);

        var currentHours = timeSincePlacement.TotalHours;

        switch (brandDelivery.Status)
        {
            case DeliveryStatus.Pending:
                // Transition to Preparing
                if (currentHours >= delays[DeliveryStatus.Preparing])
                {
                    brandDelivery.MarkAsPreparing();
                    _logger.LogInformation(
                        "BrandDelivery {BrandDeliveryId} for Brand {BrandName} transitioned to Preparing (estimated delivery: {EstimatedDelivery})",
                        brandDelivery.Id, brandDelivery.Brand?.Name, brandDelivery.EstimatedDelivery);
                }
                break;

            case DeliveryStatus.Preparing:
                // Transition to Shipped
                if (currentHours >= delays[DeliveryStatus.Shipped])
                {
                    var trackingNumber = brandDelivery.TrackingNumber ?? GenerateTrackingNumber(brandDelivery.BrandId);
                    brandDelivery.MarkAsShipped(DateTimeOffset.Now, trackingNumber);
                    _logger.LogInformation(
                        "BrandDelivery {TrackingNumber} for Brand {BrandName} transitioned to Shipped",
                        brandDelivery.TrackingNumber, brandDelivery.Brand?.Name);

                    await SendBrandDeliveryShippedEmailAsync(order, brandDelivery, scope);
                }
                break;

            case DeliveryStatus.Shipped:
                // Transition to InTransit
                if (currentHours >= delays[DeliveryStatus.InTransit])
                {
                    brandDelivery.MarkAsInTransit();
                    _logger.LogInformation(
                        "BrandDelivery {TrackingNumber} for Brand {BrandName} transitioned to InTransit",
                        brandDelivery.TrackingNumber, brandDelivery.Brand?.Name);

                    await SendBrandDeliveryInTransitEmailAsync(order, brandDelivery, scope);
                }
                break;

            case DeliveryStatus.InTransit:
                // Transition to OutForDelivery
                if (currentHours >= delays[DeliveryStatus.OutForDelivery])
                {
                    brandDelivery.MarkAsOutForDelivery();
                    _logger.LogInformation(
                        "BrandDelivery {TrackingNumber} for Brand {BrandName} transitioned to OutForDelivery",
                        brandDelivery.TrackingNumber, brandDelivery.Brand?.Name);

                    await SendBrandDeliveryOutForDeliveryEmailAsync(order, brandDelivery, scope);
                }
                break;

            case DeliveryStatus.OutForDelivery:
                // Transition to Delivered
                if (currentHours >= delays[DeliveryStatus.Delivered])
                {
                    brandDelivery.MarkAsDelivered();
                    _logger.LogInformation(
                        "BrandDelivery {TrackingNumber} for Brand {BrandName} delivered",
                        brandDelivery.TrackingNumber, brandDelivery.Brand?.Name);

                    await SendBrandDeliveryDeliveredEmailAsync(order, brandDelivery, scope);
                }
                break;
        }
    }

    private string GenerateTrackingNumber(long brandId)
    {
        return $"IND-B{brandId}-{DateTimeOffset.UtcNow.Ticks % 100000000:D8}";
    }

    private async Task SendOrderProcessingEmailAsync(Order order, IServiceScope scope)
    {
        try
        {
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var templateService = scope.ServiceProvider.GetRequiredService<IOrderEmailTemplateService>();

            var html = templateService.GenerateOrderProcessingEmail(order, order.User);
            await emailService.SendEmailAsync(
                order.User.Email,
                $"Votre commande #{order.Id} est en cours de traitement",
                html);
            _logger.LogInformation("Order processing email sent for Order {OrderId} to {Email}",
                order.Id, order.User.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order processing email for Order {OrderId}", order.Id);
        }
    }

    private async Task SendBrandDeliveryShippedEmailAsync(Order order, BrandDelivery brandDelivery, IServiceScope scope)
    {
        try
        {
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var templateService = scope.ServiceProvider.GetRequiredService<IOrderEmailTemplateService>();

            var html = templateService.GenerateOrderShippedEmail(order, order.User, brandDelivery);
            await emailService.SendEmailAsync(
                order.User.Email,
                $"Colis {brandDelivery.Brand?.Name} expédié - Commande #{order.Id}",
                html);
            _logger.LogInformation("Brand delivery shipped email sent for Order {OrderId}, Brand {BrandName}",
                order.Id, brandDelivery.Brand?.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send brand delivery shipped email for Order {OrderId}", order.Id);
        }
    }

    private async Task SendBrandDeliveryInTransitEmailAsync(Order order, BrandDelivery brandDelivery, IServiceScope scope)
    {
        try
        {
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var templateService = scope.ServiceProvider.GetRequiredService<IOrderEmailTemplateService>();

            var html = templateService.GenerateOrderInTransitEmail(order, order.User, brandDelivery);
            await emailService.SendEmailAsync(
                order.User.Email,
                $"Colis {brandDelivery.Brand?.Name} en transit - Commande #{order.Id}",
                html);
            _logger.LogInformation("Brand delivery in transit email sent for Order {OrderId}, Brand {BrandName}",
                order.Id, brandDelivery.Brand?.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send brand delivery in transit email for Order {OrderId}", order.Id);
        }
    }

    private async Task SendBrandDeliveryOutForDeliveryEmailAsync(Order order, BrandDelivery brandDelivery, IServiceScope scope)
    {
        try
        {
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var templateService = scope.ServiceProvider.GetRequiredService<IOrderEmailTemplateService>();

            var html = templateService.GenerateOrderOutForDeliveryEmail(order, order.User, brandDelivery);
            await emailService.SendEmailAsync(
                order.User.Email,
                $"Colis {brandDelivery.Brand?.Name} arrive aujourd'hui - Commande #{order.Id}",
                html);
            _logger.LogInformation("Brand delivery out for delivery email sent for Order {OrderId}, Brand {BrandName}",
                order.Id, brandDelivery.Brand?.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send brand delivery out for delivery email for Order {OrderId}", order.Id);
        }
    }

    private async Task SendBrandDeliveryDeliveredEmailAsync(Order order, BrandDelivery brandDelivery, IServiceScope scope)
    {
        try
        {
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var templateService = scope.ServiceProvider.GetRequiredService<IOrderEmailTemplateService>();

            var html = templateService.GenerateOrderDeliveredEmail(order, order.User, brandDelivery);
            await emailService.SendEmailAsync(
                order.User.Email,
                $"Colis {brandDelivery.Brand?.Name} livré - Commande #{order.Id}",
                html);
            _logger.LogInformation("Brand delivery delivered email sent for Order {OrderId}, Brand {BrandName}",
                order.Id, brandDelivery.Brand?.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send brand delivery delivered email for Order {OrderId}", order.Id);
        }
    }
}
