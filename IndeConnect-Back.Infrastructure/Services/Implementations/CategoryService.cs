using IndeConnect_Back.Application.DTOs.Categories;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly ITranslationService _translationService;

    public CategoryService(AppDbContext context, ITranslationService translationService)
    {
        _context = context;
        _translationService = translationService;
    }

    public async Task<IEnumerable<CategoryLookupDto>> GetAllCategoriesAsync()
    {
        var lang = _translationService.GetCurrentLanguage();

        var categories = await _context.Categories
            .Include(c => c.Translations) // ✅ Charger les traductions
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new CategoryLookupDto(
            c.Id,
            _translationService.GetTranslatedValue(c.Translations, lang, t => t.Name, c.Name) // ✅ Traduit
        ));
    }
}