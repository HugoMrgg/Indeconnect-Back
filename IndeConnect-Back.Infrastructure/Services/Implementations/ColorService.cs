using IndeConnect_Back.Application.DTOs.Colors;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class ColorService : IColorService
{
    private readonly AppDbContext _context;
    private readonly ITranslationService _translationService;

    public ColorService(AppDbContext context, ITranslationService translationService)
    {
        _context = context;
        _translationService = translationService;
    }

    public async Task<IEnumerable<ColorLookupDto>> GetAllColorsAsync()
    {
        var lang = _translationService.GetCurrentLanguage();

        var colors = await _context.Colors
            .Include(c => c.Translations) // ✅ Charger les traductions
            .OrderBy(c => c.Name)
            .ToListAsync();

        return colors.Select(c => new ColorLookupDto(
            c.Id,
            _translationService.GetTranslatedValue(c.Translations, lang, t => t.Name, c.Name), // ✅ Traduit
            c.Hexa
        ));
    }
}