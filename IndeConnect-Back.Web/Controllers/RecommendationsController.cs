using IndeConnect_Back.Application.DTOs.Recommendations;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    /// <summary>
    /// Recommande des produits similaires basés sur l'historique d'achat de l'utilisateur.
    /// Les préférences de couleur sont contextuelles par catégorie (ex: hoodies rouges ≠ pantalons rouges).
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <param name="limit">Nombre maximum de recommandations (par défaut: 10)</param>
    /// <returns>Liste de produits recommandés avec scores et raisons</returns>
    [HttpGet("similar/{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<RecommendedProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RecommendedProductDto>>> GetSimilarProducts(
        [FromRoute] long userId,
        [FromQuery] int limit = 10)
    {
        if (limit <= 0 || limit > 50)
        {
            return BadRequest(new { message = "Limit must be between 1 and 50" });
        }

        var recommendations = await _recommendationService.GetSimilarProductsBasedOnHistoryAsync(userId, limit);
        return Ok(recommendations);
    }

    /// <summary>
    /// Recommande des produits fréquemment achetés ensemble avec le produit spécifié.
    /// Analyse les patterns d'achats de tous les utilisateurs.
    /// </summary>
    /// <param name="productId">ID du produit de référence</param>
    /// <param name="limit">Nombre maximum de recommandations (par défaut: 5)</param>
    /// <returns>Liste de produits fréquemment achetés ensemble</returns>
    [HttpGet("frequently-bought-together/{productId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<RecommendedProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RecommendedProductDto>>> GetFrequentlyBoughtTogether(
        [FromRoute] long productId,
        [FromQuery] int limit = 5)
    {
        if (limit <= 0 || limit > 20)
        {
            return BadRequest(new { message = "Limit must be between 1 and 20" });
        }

        var recommendations = await _recommendationService.GetFrequentlyBoughtTogetherAsync(productId, limit);
        return Ok(recommendations);
    }
}
