using IndeConnect_Back.Application.DTOs.Recommendations;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.product;
using IndeConnect_Back.Domain.order;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class RecommendationService : IRecommendationService
{
    private readonly AppDbContext _context;

    public RecommendationService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Algorithme 1: Recommande des produits similaires basés sur l'historique d'achat.
    /// Les préférences de couleur sont contextuelles par catégorie.
    /// </summary>
    public async Task<IEnumerable<RecommendedProductDto>> GetSimilarProductsBasedOnHistoryAsync(long userId, int limit = 10)
    {
        // 1. Récupérer l'historique des commandes de l'utilisateur (uniquement les commandes livrées/payées)
        var userOrderItems = await _context.Orders
            .Where(o => o.UserId == userId && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Paid))
            .SelectMany(o => o.Items)
            .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
            .Include(oi => oi.Product)
                .ThenInclude(p => p.Brand)
            .Include(oi => oi.Product)
                .ThenInclude(p => p.PrimaryColor)
            .ToListAsync();
        if (!userOrderItems.Any())
        {
            // Si l'utilisateur n'a pas d'historique, retourner des produits populaires
            return await GetPopularProductsAsync(limit);
        }

        // 2. Extraire les IDs des produits déjà commandés (à exclure)
        var purchasedProductIds = userOrderItems.Select(oi => oi.ProductId).Distinct().ToHashSet();

        // 3. Analyser les préférences PAR CATÉGORIE
        var categoryPreferences = userOrderItems
            .GroupBy(oi => oi.Product.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                CategoryName = g.First().Product.Category.Name,
                // Couleurs préférées pour CETTE catégorie
                PreferredColorIds = g
                    .Where(oi => oi.Product.PrimaryColorId.HasValue)
                    .GroupBy(oi => oi.Product.PrimaryColorId!.Value)
                    .OrderByDescending(cg => cg.Count())
                    .Select(cg => cg.Key)
                    .ToList(),
                // Marques préférées
                PreferredBrandIds = g
                    .GroupBy(oi => oi.Product.BrandId)
                    .OrderByDescending(bg => bg.Count())
                    .Select(bg => bg.Key)
                    .ToList(),
                // Prix moyen dans cette catégorie
                AveragePrice = g.Average(oi => oi.UnitPrice),
                OrderCount = g.Count()
            })
            .OrderByDescending(cp => cp.OrderCount) // Priorité aux catégories les plus achetées
            .ToList();

        // 4. Récupérer tous les produits disponibles (non achetés)
        var availableProducts = await _context.Products
            .Where(p => p.IsEnabled &&
                        p.Status == ProductStatus.Online &&
                        !purchasedProductIds.Contains(p.Id))
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.PrimaryColor)
            .Include(p => p.Media)
            .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Enabled))
            .Include(p => p.Sale)
            .ToListAsync();

        // 5. Scorer chaque produit selon les préférences
        var scoredProducts = new List<(RecommendedProductDto Product, double Score, string Reason)>();

        foreach (var product in availableProducts)
        {
            var score = 0.0;
            var reasons = new List<string>();

            // Trouver les préférences pour la catégorie de ce produit
            var categoryPref = categoryPreferences.FirstOrDefault(cp => cp.CategoryId == product.CategoryId);

            if (categoryPref != null)
            {
                // +5 points : même catégorie ET couleur préférée pour cette catégorie
                if (product.PrimaryColorId.HasValue &&
                    categoryPref.PreferredColorIds.Contains(product.PrimaryColorId.Value))
                {
                    score += 5;
                    reasons.Add($"Couleur préférée pour {categoryPref.CategoryName}");
                }
                // +3 points : même catégorie (même si couleur différente)
                else
                {
                    score += 3;
                    reasons.Add($"Catégorie {categoryPref.CategoryName}");
                }

                // +2 points : marque préférée
                if (categoryPref.PreferredBrandIds.Contains(product.BrandId))
                {
                    score += 2;
                    reasons.Add("Marque préférée");
                }

                // +1 point : prix dans la gamme habituelle (±30%)
                var currentPrice = product.CalculateCurrentPrice();
                var priceDifference = Math.Abs(currentPrice - categoryPref.AveragePrice) / categoryPref.AveragePrice;
                if (priceDifference <= 0.3m)
                {
                    score += 1;
                    reasons.Add("Prix habituel");
                }
            }

            // +1 point : bon rating (>= 4.0)
            var avgRating = product.GetAverageRating();
            if (avgRating >= 4.0)
            {
                score += 1;
                reasons.Add("Bien noté");
            }

            // +0.5 point : en promotion
            if (product.Sale != null && product.Sale.IsActive)
            {
                score += 0.5;
                reasons.Add("En promotion");
            }

            if (score > 0)
            {
                var dto = MapToRecommendedProductDto(product, score, string.Join(", ", reasons));
                scoredProducts.Add((dto, score, string.Join(", ", reasons)));
            }
        }

        // 6. Retourner top N produits par score
        return scoredProducts
            .OrderByDescending(sp => sp.Score)
            .ThenByDescending(sp => sp.Product.AverageRating)
            .Take(limit)
            .Select(sp => sp.Product);
    }

    /// <summary>
    /// Algorithme 2: Recommande des produits fréquemment achetés ensemble.
    /// Analyse les patterns d'achats de tous les utilisateurs.
    /// </summary>
    public async Task<IEnumerable<RecommendedProductDto>> GetFrequentlyBoughtTogetherAsync(long productId, int limit = 5)
    {
        // 1. Vérifier que le produit existe
        var targetProduct = await _context.Products.FindAsync(productId);
        if (targetProduct == null)
        {
            return Enumerable.Empty<RecommendedProductDto>();
        }

        // 2. Trouver toutes les commandes contenant ce produit
        var ordersWithProduct = await _context.OrderItems
            .Where(oi => oi.ProductId == productId)
            .Select(oi => oi.OrderId)
            .Distinct()
            .ToListAsync();

        if (!ordersWithProduct.Any())
        {
            return Enumerable.Empty<RecommendedProductDto>();
        }

        // 3. Trouver tous les autres produits dans ces commandes
        var coOccurrences = await _context.OrderItems
            .Where(oi => ordersWithProduct.Contains(oi.OrderId) && oi.ProductId != productId)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                CoOccurrenceCount = g.Select(oi => oi.OrderId).Distinct().Count()
            })
            .ToListAsync();

        if (!coOccurrences.Any())
        {
            return Enumerable.Empty<RecommendedProductDto>();
        }

        // 4. Calculer le score de co-occurrence (pourcentage)
        var totalOrdersWithTargetProduct = ordersWithProduct.Count;

        var scoredCoOccurrences = coOccurrences
            .Select(co => new
            {
                co.ProductId,
                Score = (double)co.CoOccurrenceCount / totalOrdersWithTargetProduct * 100, // Pourcentage
                Reason = $"Acheté ensemble dans {co.CoOccurrenceCount}/{totalOrdersWithTargetProduct} commandes ({Math.Round((double)co.CoOccurrenceCount / totalOrdersWithTargetProduct * 100, 1)}%)"
            })
            .OrderByDescending(co => co.Score)
            .Take(limit)
            .ToList();

        // 5. Récupérer les détails des produits
        var productIds = scoredCoOccurrences.Select(sco => sco.ProductId).ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.IsEnabled && p.Status == ProductStatus.Online)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.PrimaryColor)
            .Include(p => p.Media)
            .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Enabled))
            .Include(p => p.Sale)
            .ToListAsync();

        // 6. Mapper vers DTO avec le score de co-occurrence
        var recommendations = new List<RecommendedProductDto>();

        foreach (var product in products)
        {
            var scoreInfo = scoredCoOccurrences.First(sco => sco.ProductId == product.Id);
            var dto = MapToRecommendedProductDto(product, scoreInfo.Score, scoreInfo.Reason);
            recommendations.Add(dto);
        }

        return recommendations.OrderByDescending(r => r.RecommendationScore);
    }

    /// <summary>
    /// Retourne des produits populaires (fallback quand l'utilisateur n'a pas d'historique)
    /// </summary>
    private async Task<IEnumerable<RecommendedProductDto>> GetPopularProductsAsync(int limit)
    {
        var popularProducts = await _context.Products
            .Where(p => p.IsEnabled && p.Status == ProductStatus.Online)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.PrimaryColor)
            .Include(p => p.Media)
            .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Enabled))
            .Include(p => p.Sale)
            .OrderByDescending(p => p.Reviews.Count(r => r.Status == ReviewStatus.Enabled))
            .ThenByDescending(p => p.Reviews.Where(r => r.Status == ReviewStatus.Enabled).Average(r => (double?)r.Rating) ?? 0)
            .Take(limit)
            .ToListAsync();

        return popularProducts.Select(p => MapToRecommendedProductDto(p, 0, "Produit populaire"));
    }

    /// <summary>
    /// Mapper un Product vers RecommendedProductDto
    /// </summary>
    private RecommendedProductDto MapToRecommendedProductDto(Product product, double score, string reason)
    {
        var basePrice = product.Price;
        var currentPrice = product.CalculateCurrentPrice();
        var salePrice = currentPrice != basePrice ? currentPrice : (decimal?)null;

        return new RecommendedProductDto(
            product.Id,
            product.Name,
            product.GetPrimaryImageUrl(),
            basePrice,
            salePrice,
            product.Description,
            product.GetAverageRating(),
            product.GetApprovedReviewsCount(),
            product.Brand?.Name,
            product.Category?.Name,
            product.PrimaryColor?.Name,
            Math.Round(score, 2),
            reason
        );
    }
}
