using IndeConnect_Back.Application.DTOs.Orders;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IOrderEmailTemplateService _templateService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        AppDbContext context,
        IEmailService emailService,
        IOrderEmailTemplateService templateService,
        ILogger<OrderService> logger)
    {
        _context = context;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderAsync(long userId, CreateOrderRequest request)
    {
        // Récupérer le panier de l'utilisateur
        var cart = await _context.Carts
            .Include(c => c.User)
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p.Brand)
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Le panier est vide");
        }

        // Vérifier que l'adresse appartient à l'utilisateur
        var address = await _context.ShippingAddresses
            .FirstOrDefaultAsync(a => a.Id == request.ShippingAddressId && a.UserId == userId);

        if (address == null)
        {
            throw new InvalidOperationException("Adresse de livraison invalide");
        }

        // Grouper les items par marque
        var itemsByBrand = cart.Items
            .GroupBy(i => i.Product.BrandId)
            .ToList();

        // Créer les OrderItems (sans les ajouter à la commande tout de suite)
        var orderItemsByBrand = new Dictionary<long, List<OrderItem>>();

        foreach (var brandGroup in itemsByBrand)
        {
            var brandId = brandGroup.Key;
            var brandOrderItems = new List<OrderItem>();

            foreach (var cartItem in brandGroup)
            {
                var orderItem = new OrderItem(
                    productId: cartItem.ProductId,
                    productName: cartItem.Product.Name,
                    quantity: cartItem.Quantity,
                    unitPrice: cartItem.UnitPrice,
                    variantId: cartItem.ProductVariantId
                );

                brandOrderItems.Add(orderItem);
            }

            orderItemsByBrand[brandId] = brandOrderItems;
        }

        // Créer la commande avec tous les items
        var allOrderItems = orderItemsByBrand.Values.SelectMany(items => items).ToList();
        var order = new Order(
            userId: userId,
            shippingAddressId: request.ShippingAddressId,
            items: allOrderItems,
            currency: "EUR"
        );

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); 
        
        // Valider et charger les méthodes de livraison choisies
        var deliveryChoicesMap = request.DeliveryChoices.ToDictionary(dc => dc.BrandId, dc => dc.ShippingMethodId);

        // Vérifier que toutes les marques ont une méthode de livraison
        foreach (var brandGroup in itemsByBrand)
        {
            if (!deliveryChoicesMap.ContainsKey(brandGroup.Key))
            {
                throw new InvalidOperationException($"Aucune méthode de livraison sélectionnée pour la marque {brandGroup.Key}");
            }
        }

        // Charger toutes les méthodes de livraison en une seule requête
        var shippingMethodIds = deliveryChoicesMap.Values.ToList();
        var shippingMethods = await _context.BrandShippingMethods
            .Where(sm => shippingMethodIds.Contains(sm.Id))
            .ToDictionaryAsync(sm => sm.Id);

        // Créer une BrandDelivery et une Invoice par marque
        decimal totalShippingFees = 0m;

        foreach (var brandGroup in itemsByBrand)
        {
            var brandId = brandGroup.Key;
            var brandItems = orderItemsByBrand[brandId];
            var brandTotal = brandItems.Sum(i => i.UnitPrice * i.Quantity);

            // Récupérer la méthode de livraison choisie
            var shippingMethodId = deliveryChoicesMap[brandId];
            if (!shippingMethods.TryGetValue(shippingMethodId, out var shippingMethod))
            {
                throw new InvalidOperationException($"Méthode de livraison {shippingMethodId} introuvable");
            }

            // Vérifier que la méthode appartient bien à la marque
            if (shippingMethod.BrandId != brandId)
            {
                throw new InvalidOperationException($"La méthode de livraison {shippingMethodId} n'appartient pas à la marque {brandId}");
            }

            // Charger la marque avec ses dépôts pour calculer l'estimation
            var brand = await _context.Brands
                .Include(b => b.Deposits)
                .FirstOrDefaultAsync(b => b.Id == brandId);

            // Créer la BrandDelivery avec la méthode de livraison et les frais
            var brandDelivery = new BrandDelivery(
                brandId: brandId,
                orderId: order.Id,
                description: $"Livraison {brand?.Name ?? "Marque " + brandId}",
                shippingMethodId: shippingMethod.Id,
                shippingFee: shippingMethod.Price
            );

            totalShippingFees += shippingMethod.Price;

            // Calculer l'estimation de livraison basée sur la distance ET la méthode de livraison
            // Utiliser le premier dépôt de la marque (ou le plus proche dans une vraie app)
            var deposit = brand?.Deposits.FirstOrDefault();
            if (deposit != null)
            {
                var estimatedDelivery = DeliveryEstimator.CalculateEstimatedDeliveryDate(
                    deposit,
                    address,
                    order.PlacedAt,
                    shippingMethod
                );
                brandDelivery.SetEstimatedDelivery(estimatedDelivery);
            }
            else
            {
                // Fallback si pas de dépôt
                var fallbackHours = 28 + (shippingMethod.EstimatedMaxDays * 24);
                brandDelivery.SetEstimatedDelivery(order.PlacedAt.AddHours(fallbackHours));
            }

            _context.BrandDeliveries.Add(brandDelivery);

            await _context.SaveChangesAsync();

            // Lier les OrderItems à cette BrandDelivery
            foreach (var orderItem in brandItems)
            {
                orderItem.AssignToBrandDelivery(brandDelivery.Id);
                brandDelivery.AddItem(orderItem);
            }

            // Ajouter la BrandDelivery à la commande
            order.AddBrandDelivery(brandDelivery);

            // Créer la facture pour cette marque (produits + frais de port)
            var invoiceAmount = brandTotal + shippingMethod.Price;
            var invoice = new Invoice(
                orderId: order.Id,
                brandId: brandId,
                invoiceNumber: InvoiceNumberGenerator.Generate(order.Id, brandId),
                amount: invoiceAmount
            );

            order.AddInvoice(invoice);
            _context.Invoices.Add(invoice);
        }

        // Décrémenter les stocks
        foreach (var cartItem in cart.Items)
        {
            if (cartItem.ProductVariant != null)
            {
                cartItem.ProductVariant.DecrementStock(cartItem.Quantity);

                if (cartItem.ProductVariant.StockCount < 0)
                {
                    throw new InvalidOperationException(
                        $"Stock insuffisant pour {cartItem.Product.Name}"
                    );
                }
            }
        }

        // Vider le panier
        _context.CartItems.RemoveRange(cart.Items);

        await _context.SaveChangesAsync();

        // Envoyer l'email de confirmation de commande
        try
        {
            var html = _templateService.GenerateOrderConfirmationEmail(order, cart.User, address);
            await _emailService.SendEmailAsync(
                cart.User.Email,
                $"Confirmation de votre commande #{order.Id}",
                html);
            _logger.LogInformation("Order confirmation email sent for Order {OrderId} to {Email}",
                order.Id, cart.User.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email for Order {OrderId}", order.Id);
        }

        // Retourner l'ordre créé
        return await GetOrderByIdAsync(order.Id)
               ?? throw new InvalidOperationException("Erreur lors de la récupération de la commande");
    }

    public async Task<OrderDto?> GetOrderByIdAsync(long orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Invoices)
            .ThenInclude(i => i.Brand)
            .Include(o => o.BrandDeliveries)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return null;

        return MapToDto(order);
    }

    public async Task<OrderTrackingDto?> GetOrderTrackingAsync(long orderId)
    {
        var order = await _context.Orders
            .Include(o => o.BrandDeliveries)
                .ThenInclude(bd => bd.Brand)
            .Include(o => o.BrandDeliveries)
                .ThenInclude(bd => bd.Items)
                    .ThenInclude(i => i.Product)
            .Include(o => o.BrandDeliveries)
                .ThenInclude(bd => bd.Items)
                    .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return null;

        // Créer le tracking par marque
        var deliveriesByBrand = new List<BrandDeliveryTrackingDto>();

        foreach (var brandDelivery in order.BrandDeliveries)
        {
            var timeline = BuildBrandDeliveryTimeline(order, brandDelivery);

            var brandTrackingDto = new BrandDeliveryTrackingDto
            {
                BrandDeliveryId = brandDelivery.Id,
                BrandId = brandDelivery.BrandId,
                BrandName = brandDelivery.Brand?.Name ?? "Marque inconnue",
                BrandLogoUrl = brandDelivery.Brand?.LogoUrl,
                Status = brandDelivery.Status,
                TrackingNumber = brandDelivery.TrackingNumber,
                Items = brandDelivery.Items.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList(),
                TotalAmount = brandDelivery.Items.Sum(i => i.Quantity * i.UnitPrice),
                CreatedAt = brandDelivery.CreatedAt,
                ShippedAt = brandDelivery.ShippedAt,
                DeliveredAt = brandDelivery.DeliveredAt,
                EstimatedDelivery = brandDelivery.EstimatedDelivery,
                Timeline = timeline
            };

            deliveriesByBrand.Add(brandTrackingDto);
        }

        // Calculer la date de livraison estimée la plus tardive
        var latestEstimatedDelivery = deliveriesByBrand
            .Where(d => d.EstimatedDelivery.HasValue)
            .Select(d => d.EstimatedDelivery!.Value)
            .OrderByDescending(d => d)
            .FirstOrDefault();

        return new OrderTrackingDto
        {
            OrderId = order.Id,
            GlobalStatus = order.Status,
            PlacedAt = order.PlacedAt,
            TotalAmount = order.TotalAmount,
            DeliveriesByBrand = deliveriesByBrand,
            LatestEstimatedDelivery = latestEstimatedDelivery != default ? latestEstimatedDelivery : null
        };
    }

    /// <summary>
    /// Construit la timeline de suivi pour une BrandDelivery spécifique.
    /// Utilise la méthode du domaine et mappe vers les DTOs.
    /// </summary>
    private List<TrackingStepDto> BuildBrandDeliveryTimeline(Order order, BrandDelivery delivery)
    {
        // Utiliser la méthode du domaine pour construire la timeline
        var domainTimeline = delivery.BuildTrackingTimeline(order);

        // Mapper vers les DTOs
        return domainTimeline.Select(step => new TrackingStepDto
        {
            Status = step.Status,
            Label = step.Label,
            Description = step.Description,
            CompletedAt = step.CompletedAt,
            IsCompleted = step.IsCompleted,
            IsCurrent = step.IsCurrent
        }).ToList();
    }



    public async Task<List<OrderDto>> GetUserOrdersAsync(long userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Invoices)
            .ThenInclude(i => i.Brand)
            .Include(o => o.BrandDeliveries)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    // Mapper qui respecte l'encapsulation de ton Domain
    private OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            ShippingAddressId = order.ShippingAddressId,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            PlacedAt = order.PlacedAt,
            Currency = order.Currency,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                VariantId = i.VariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            Invoices = order.Invoices.Select(inv => new InvoiceDto
            {
                Id = inv.Id,
                BrandId = inv.BrandId,
                InvoiceNumber = inv.InvoiceNumber,
                Amount = inv.Amount,
                IssuedAt = inv.IssuedAt
            }).ToList()
        };
    }

}