using IndeConnect_Back.Application.DTOs.Users;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface ICartService
{
    Task<CartDto> GetUserCartAsync(long userId);

    /// <summary>
    /// Ajoute une variante de produit au panier de l'utilisateur.
    /// Retourne null si la variante n'existe pas.
    /// </summary>
    Task<CartDto?> AddVariantToCartAsync(long userId, long variantId, int quantity);
    
    /// <summary>
    /// Retire une variante du panier ou diminue sa quantité.
    /// Si quantity est null, retire complètement l'item.
    /// Si quantity est fourni, diminue la quantité de l'item.
    /// Retourne null si le panier ou la variante n'existe pas.
    /// </summary>
    Task<CartDto?> RemoveVariantFromCartAsync(long userId, long variantId, int? quantity = null);
    
    /// <summary>
    /// Vide complètement le panier de l'utilisateur.
    /// </summary>
    Task ClearCartAsync(long userId);
}