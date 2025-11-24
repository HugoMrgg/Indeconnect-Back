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
}