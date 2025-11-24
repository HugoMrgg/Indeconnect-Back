using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class EthicsService : IEthicsService
{
    private readonly AppDbContext _context;

    public EthicsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EthicTagsListResponse> GetAllEthicTagsAsync()
    {
        // Récupérer tous les tags uniques des marques approuvées
        var tags = await _context.BrandEthicTags
            .Include(bet => bet.Brand)
            .Where(bet => bet.Brand.Status == BrandStatus.Approved)
            .GroupBy(bet => bet.TagKey)
            .Select(g => new
            {
                Key = g.Key,
                BrandCount = g.Count()
            })
            .ToListAsync();

        // Mapper vers le DTO avec labels explicites
        var tagDtos = tags.Select(t => new EthicTagDto(
            t.Key,
            GetTagLabel(t.Key),
            GetTagDescription(t.Key),
            t.BrandCount
        )).OrderBy(t => t.Label);

        return new EthicTagsListResponse(tagDtos);
    }

    /// <summary>
    /// Convertit la clé technique en label lisible
    /// À adapter selon tes tags réels dans la DB
    /// </summary>
    private string GetTagLabel(string key)
    {
        return key switch
        {
            "organic" => "Bio",
            "fair-trade" => "Commerce équitable",
            "vegan" => "Vegan",
            "local" => "Production locale",
            "recycled" => "Matériaux recyclés",
            "eco-friendly" => "Éco-responsable",
            "handmade" => "Fait main",
            "sustainable" => "Durable",
            "ethical" => "Éthique",
            "cruelty-free" => "Sans cruauté animale",
            _ => key // Par défaut, retourner la clé telle quelle
        };
    }

    /// <summary>
    /// Retourne une description optionnelle du tag
    /// </summary>
    private string? GetTagDescription(string key)
    {
        return key switch
        {
            "organic" => "Produits certifiés biologiques sans pesticides",
            "fair-trade" => "Commerce équitable garantissant des conditions de travail justes",
            "vegan" => "Aucun produit d'origine animale",
            "local" => "Production locale pour réduire l'empreinte carbone",
            "recycled" => "Fabriqué à partir de matériaux recyclés",
            _ => null
        };
    }
}
