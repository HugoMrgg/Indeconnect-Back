using IndeConnect_Back.Application.DTOs.Orders;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(long userId, CreateOrderRequest request)
    {
        // Récupérer le panier de l'utilisateur
        var cart = await _context.Carts
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

        // Créer les OrderItems en utilisant ton Domain Model
        var orderItems = new List<OrderItem>();

        foreach (var cartItem in cart.Items)
        {
            var orderItem = new OrderItem(
                productId: cartItem.ProductId,
                productName: cartItem.Product.Name,
                quantity: cartItem.Quantity,
                unitPrice: cartItem.UnitPrice,
                variantId: cartItem.ProductVariantId
            );

            orderItems.Add(orderItem);
        }

        // Créer la commande avec ton constructeur DDD
        var order = new Order(
            userId: userId,
            shippingAddressId: request.ShippingAddressId,
            items: orderItems,
            currency: "EUR"
        );

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // Pour avoir l'ID

        // Créer les factures par marque avec ton Domain
        foreach (var brandGroup in itemsByBrand)
        {
            var brandId = brandGroup.Key;
            var brandItems = brandGroup.ToList();
            var brandTotal = brandItems.Sum(i => i.UnitPrice * i.Quantity);

            var invoice = new Invoice(
                orderId: order.Id,
                brandId: brandId,
                invoiceNumber: GenerateInvoiceNumber(order.Id, brandId),
                amount: brandTotal
            );

            _context.Invoices.Add(invoice);
        }

        // Créer les Deliveries avec ton Domain
        foreach (var choice in request.DeliveryChoices)
        {
            var delivery = new Delivery(
                description: $"Livraison pour la marque {choice.BrandId}",
                orderId: order.Id,
                trackingNumber: null // Sera ajouté plus tard via BPost
            );

            _context.Deliveries.Add(delivery);
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
            .Include(o => o.Deliveries)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return null;

        return MapToDto(order);
    }

    public async Task<List<OrderDto>> GetUserOrdersAsync(long userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Invoices)
            .ThenInclude(i => i.Brand)
            .Include(o => o.Deliveries)
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

    private string GenerateInvoiceNumber(long orderId, long brandId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        return $"INV-{timestamp}-{orderId}-{brandId}";
    }
}