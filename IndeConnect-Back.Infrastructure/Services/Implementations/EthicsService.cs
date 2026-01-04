using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class EthicsService : IEthicsService
{
    private readonly AppDbContext _context;
    private readonly ITranslationService _translationService;

    public EthicsService(AppDbContext context, ITranslationService translationService)
    {
        _context = context;
        _translationService = translationService;
    }

    public async Task<EthicTagsListResponse> GetAllEthicTagsAsync()
    {
        var lang = _translationService.GetCurrentLanguage();

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

        // Mapper vers le DTO avec labels traduits
        var tagDtos = tags.Select(t => new EthicTagDto(
            t.Key,
            GetTagLabel(t.Key, lang), // ✅ Passer la langue
            GetTagDescription(t.Key, lang), // ✅ Passer la langue
            t.BrandCount
        )).OrderBy(t => t.Label);

        return new EthicTagsListResponse(tagDtos);
    }

    /// <summary>
    /// Convertit la clé technique en label lisible selon la langue
    /// </summary>
    private string GetTagLabel(string key, string lang)
    {
        return (key, lang) switch
        {
            // Français
            ("organic", "fr") => "Bio",
            ("fair-trade", "fr") => "Commerce équitable",
            ("vegan", "fr") => "Vegan",
            ("local", "fr") => "Production locale",
            ("recycled", "fr") => "Matériaux recyclés",
            ("eco-friendly", "fr") => "Éco-responsable",
            ("handmade", "fr") => "Fait main",
            ("sustainable", "fr") => "Durable",
            ("ethical", "fr") => "Éthique",
            ("cruelty-free", "fr") => "Sans cruauté animale",

            // Nederlands
            ("organic", "nl") => "Biologisch",
            ("fair-trade", "nl") => "Eerlijke handel",
            ("vegan", "nl") => "Veganistisch",
            ("local", "nl") => "Lokale productie",
            ("recycled", "nl") => "Gerecycled materiaal",
            ("eco-friendly", "nl") => "Milieuvriendelijk",
            ("handmade", "nl") => "Handgemaakt",
            ("sustainable", "nl") => "Duurzaam",
            ("ethical", "nl") => "Ethisch",
            ("cruelty-free", "nl") => "Zonder dierenleed",

            // Deutsch
            ("organic", "de") => "Bio",
            ("fair-trade", "de") => "Fairer Handel",
            ("vegan", "de") => "Vegan",
            ("local", "de") => "Lokale Produktion",
            ("recycled", "de") => "Recyceltes Material",
            ("eco-friendly", "de") => "Umweltfreundlich",
            ("handmade", "de") => "Handgemacht",
            ("sustainable", "de") => "Nachhaltig",
            ("ethical", "de") => "Ethisch",
            ("cruelty-free", "de") => "Tierversuchsfrei",

            // English
            ("organic", "en") => "Organic",
            ("fair-trade", "en") => "Fair Trade",
            ("vegan", "en") => "Vegan",
            ("local", "en") => "Local Production",
            ("recycled", "en") => "Recycled Materials",
            ("eco-friendly", "en") => "Eco-Friendly",
            ("handmade", "en") => "Handmade",
            ("sustainable", "en") => "Sustainable",
            ("ethical", "en") => "Ethical",
            ("cruelty-free", "en") => "Cruelty-Free",

            _ => key // Fallback
        };
    }

    /// <summary>
    /// Retourne une description optionnelle du tag selon la langue
    /// </summary>
    private string? GetTagDescription(string key, string lang)
    {
        return (key, lang) switch
        {
            // Français
            ("organic", "fr") => "Produits certifiés biologiques sans pesticides",
            ("fair-trade", "fr") => "Commerce équitable garantissant des conditions de travail justes",
            ("vegan", "fr") => "Aucun produit d'origine animale",
            ("local", "fr") => "Production locale pour réduire l'empreinte carbone",
            ("recycled", "fr") => "Fabriqué à partir de matériaux recyclés",

            // Nederlands
            ("organic", "nl") => "Gecertificeerde biologische producten zonder pesticiden",
            ("fair-trade", "nl") => "Eerlijke handel met rechtvaardigte arbeidsomstandigheden",
            ("vegan", "nl") => "Geen dierlijke producten",
            ("local", "nl") => "Lokale productie om de CO2-voetafdruk te verminderen",
            ("recycled", "nl") => "Gemaakt van gerecyclede materialen",

            // Deutsch
            ("organic", "de") => "Zertifizierte Bio-Produkte ohne Pestizide",
            ("fair-trade", "de") => "Fairer Handel mit gerechten Arbeitsbedingungen",
            ("vegan", "de") => "Keine tierischen Produkte",
            ("local", "de") => "Lokale Produktion zur Reduzierung des CO2-Fußabdrucks",
            ("recycled", "de") => "Hergestellt aus recycelten Materialien",

            // English
            ("organic", "en") => "Certified organic products without pesticides",
            ("fair-trade", "en") => "Fair trade ensuring fair working conditions",
            ("vegan", "en") => "No animal-derived products",
            ("local", "en") => "Local production to reduce carbon footprint",
            ("recycled", "en") => "Made from recycled materials",

            _ => null
        };
    }
}