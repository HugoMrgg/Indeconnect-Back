using IndeConnect_Back.Application.DTOs.Recommendations;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IRecommendationService
{
    /// <summary>
    /// Recommande des produits similaires basés sur l'historique d'achat de l'utilisateur.
    /// Analyse les préférences par catégorie (couleurs, marques, prix).
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <param name="limit">Nombre maximum de recommandations (par défaut: 10)</param>
    /// <returns>Liste de produits recommandés avec scores</returns>
    Task<IEnumerable<RecommendedProductDto>> GetSimilarProductsBasedOnHistoryAsync(long userId, int limit = 10);

    /// <summary>
    /// Recommande des produits fréquemment achetés ensemble avec le produit spécifié.
    /// Analyse les patterns d'achats de tous les utilisateurs.
    /// </summary>
    /// <param name="productId">ID du produit de référence</param>
    /// <param name="limit">Nombre maximum de recommandations (par défaut: 5)</param>
    /// <returns>Liste de produits fréquemment achetés ensemble</returns>
    Task<IEnumerable<RecommendedProductDto>> GetFrequentlyBoughtTogetherAsync(long productId, int limit = 5);
}