using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

/**
 * Service gérant les opérations liées au panier (récupération, ajout de variantes, etc.).
 */
public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Récupère le panier de l'utilisateur, en le créant s'il n'existe pas encore.
    /// </summary>
    public async Task<CartDto> GetUserCartAsync(long userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart(userId);
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            // Panier vide
            return new CartDto(
                cart.Id,
                userId,
                Enumerable.Empty<CartItemDto>(),
                0,
                0m
            );
        }

        return BuildCartDto(cart);
    }

    /// <summary>
    /// Ajoute une variante de produit au panier de l'utilisateur.
    /// Retourne null si la variante n'existe pas.
    /// </summary>
    public async Task<CartDto?> AddVariantToCartAsync(long userId, long variantId, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        // Vérifier que la variante existe et charger le produit lié
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId);

        if (variant == null)
            return null; // sera transformé en 404 au niveau du controller

        var product = variant.Product;

        // Vérifier que l'utilisateur existe
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new InvalidOperationException("User not found");

        // Récupérer ou créer le panier
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart(userId);
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            // On recharge pour être sûr d'avoir les Items correctement trackés
            cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstAsync(c => c.UserId == userId);
        }

        // Chercher une ligne de panier existante pour ce produit
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == product.Id);

        if (existingItem == null)
        {
            var unitPrice = product.Price; // on prend le prix courant du produit
            var newItem   = new CartItem(cart.Id, product.Id, quantity, unitPrice);
            _context.CartItems.Add(newItem);
        }
        else
        {
            existingItem.IncreaseQuantity(quantity);
        }

        cart.MarkUpdated();
        await _context.SaveChangesAsync();

        // On renvoie l'état complet du panier après mise à jour
        return await GetUserCartAsync(userId);
    }

    private static CartDto BuildCartDto(Cart cart)
    {
        var items = cart.Items.Select(i =>
        {
            var lineTotal = i.UnitPrice * i.Quantity;

            return new CartItemDto(
                i.ProductId,
                i.Product.Name,
                i.UnitPrice,
                i.Quantity,
                lineTotal
            );
        }).ToList();

        var totalItems  = items.Sum(x => x.Quantity);
        var totalAmount = items.Sum(x => x.LineTotal);

        return new CartDto(
            cart.Id,
            cart.UserId,
            items,
            totalItems,
            totalAmount
        );
    }
}
