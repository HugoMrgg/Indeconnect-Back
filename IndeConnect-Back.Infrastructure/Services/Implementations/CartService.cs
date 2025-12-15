using IndeConnect_Back.Application.DTOs.Products;
using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.product;
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
                    .ThenInclude(p => p.Brand)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Media)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.PrimaryColor)
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Size)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart(userId);
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            
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

        // Charger la variante avec toutes les données nécessaires
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
                .ThenInclude(p => p.Brand)
            .Include(v => v.Product)
                .ThenInclude(p => p.Media)
            .Include(v => v.Product)
                .ThenInclude(p => p.PrimaryColor)
            .Include(v => v.Size)
            .FirstOrDefaultAsync(v => v.Id == variantId);

        if (variant == null)
            return null;

        var product = variant.Product;

        // Vérifier que le produit est disponible
        if (!product.IsEnabled || product.Status != ProductStatus.Online)
            throw new InvalidOperationException("Product is not available");

        // Vérifier le stock disponible
        if (variant.StockCount < quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {variant.StockCount}");

        // Vérifier que l'utilisateur existe
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new InvalidOperationException("User not found");

        // Récupérer ou créer le panier
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart(userId);
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            
            cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                .FirstAsync(c => c.UserId == userId);
        }

        // MODIFIÉ : Chercher un item existant avec le même ProductVariantId
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == variantId);

        if (existingItem == null)
        {
            // Utiliser le prix override de la variante si disponible, sinon le prix du produit
            var unitPrice = variant.PriceOverride ?? product.Price;
            
            var newItem = new CartItem(cart.Id, product.Id, variantId, quantity, unitPrice);
            _context.CartItems.Add(newItem);
        }
        else
        {
            // Vérifier que le stock est suffisant pour la nouvelle quantité totale
            var newQuantity = existingItem.Quantity + quantity;
            if (variant.StockCount < newQuantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {variant.StockCount}");
            
            existingItem.IncreaseQuantity(quantity);
        }

        cart.MarkUpdated();
        await _context.SaveChangesAsync();

        return await GetUserCartAsync(userId);
    }

    private static CartDto BuildCartDto(Cart cart)
    {
        var items = cart.Items.Select(i =>
        {
            var product = i.Product;
            var variant = i.ProductVariant;
            var lineTotal = i.UnitPrice * i.Quantity;

            // Récupérer l'image primaire du produit
            var primaryImage = product.Media
                .Where(m => m.IsPrimary)
                .OrderBy(m => m.DisplayOrder)
                .FirstOrDefault()?.Url
                ?? product.Media
                    .OrderBy(m => m.DisplayOrder)
                    .FirstOrDefault()?.Url;

            return new CartItemDto(
                i.ProductId,
                product.Name,
                product.Brand.Name,
                product.Brand.Id, 
                primaryImage,
                product.PrimaryColor != null 
                    ? new ColorDto(product.PrimaryColor.Id, product.PrimaryColor.Name, product.PrimaryColor.Hexa)
                    : null,
                variant.Size != null 
                    ? new SizeDto(variant.Size.Id, variant.Size.Name)
                    : null,
                i.ProductVariantId,
                variant.SKU,
                variant.StockCount,
                i.UnitPrice,
                i.Quantity,
                lineTotal,
                i.AddedAt
            );
        }).ToList();

        var totalItems = items.Sum(x => x.Quantity);
        var totalAmount = items.Sum(x => x.LineTotal);

        return new CartDto(
            cart.Id,
            cart.UserId,
            items,
            totalItems,
            totalAmount
        );
    }
    /// <summary>
    /// Retire une variante du panier ou diminue sa quantité.
    /// Si quantity est null, retire complètement l'item.
    /// Retourne null si le panier ou la variante n'existe pas.
    /// </summary>
    public async Task<CartDto?> RemoveVariantFromCartAsync(long userId, long variantId, int? quantity = null)
    {
        // Récupérer le panier avec tous les items
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Brand)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Media)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.PrimaryColor)
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Size)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
            return null;

        // Chercher l'item correspondant à la variante
        var cartItem = cart.Items.FirstOrDefault(i => i.ProductVariantId == variantId);
        if (cartItem == null)
            return null;

        if (quantity.HasValue && quantity.Value > 0)
        {
            // Diminuer la quantité
            var newQuantity = cartItem.Quantity - quantity.Value;
            
            if (newQuantity <= 0)
            {
                // Si la nouvelle quantité est <= 0, retirer complètement l'item
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                // Sinon, mettre à jour la quantité
                cartItem.SetQuantity(newQuantity);
            }
        }
        else
        {
            // Retirer complètement l'item
            _context.CartItems.Remove(cartItem);
        }

        cart.MarkUpdated();
        await _context.SaveChangesAsync();

        return await GetUserCartAsync(userId);
    }

    /// <summary>
    /// Vide complètement le panier de l'utilisateur.
    /// </summary>
    public async Task ClearCartAsync(long userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
            return;

        // Supprimer tous les items du panier
        _context.CartItems.RemoveRange(cart.Items);
        
        cart.MarkUpdated();
        await _context.SaveChangesAsync();
    }
}
