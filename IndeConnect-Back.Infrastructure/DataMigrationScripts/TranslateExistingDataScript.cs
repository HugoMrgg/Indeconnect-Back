using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IndeConnect_Back.Infrastructure.DataMigrationScripts;

/// <summary>
/// Script to translate all existing data (Brands, Categories, Products, Sizes, Colors)
/// from French to Dutch, German, and English using DeepL API.
///
/// Usage: Run this script once after deploying the i18n feature to populate translation tables.
/// </summary>
public class TranslateExistingDataScript
{
    private readonly AppDbContext _context;
    private readonly IAutoTranslationService _translationService;
    private readonly ILogger<TranslateExistingDataScript> _logger;
    private static readonly string[] TargetLanguages = { "nl", "de", "en" };

    public TranslateExistingDataScript(
        AppDbContext context,
        IAutoTranslationService translationService,
        ILogger<TranslateExistingDataScript> logger)
    {
        _context = context;
        _translationService = translationService;
        _logger = logger;
    }

    /// <summary>
    /// Runs the complete data translation process.
    /// This will translate: Brands, Categories, Products, Sizes, Colors
    /// </summary>
    public async Task RunAsync(bool dryRun = false)
    {
        _logger.LogInformation("=== Starting Translation Migration Script ===");
        _logger.LogInformation("Dry Run: {DryRun}", dryRun);

        try
        {
            await TranslateBrandsAsync(dryRun);
            await TranslateCategoriesAsync(dryRun);
            await TranslateProductsAsync(dryRun);
            await TranslateSizesAsync(dryRun);
            await TranslateColorsAsync(dryRun);

            if (!dryRun)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("=== Translation Migration Completed Successfully ===");
            }
            else
            {
                _logger.LogInformation("=== Dry Run Completed (No Changes Saved) ===");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation migration failed");
            throw;
        }
    }

    private async Task TranslateBrandsAsync(bool dryRun)
    {
        _logger.LogInformation("--- Translating Brands ---");

        var brands = await _context.Brands
            .Include(b => b.Translations)
            .ToListAsync();

        _logger.LogInformation("Found {Count} brands to translate", brands.Count);

        foreach (var brand in brands)
        {
            try
            {
                // Skip if already has translations
                if (brand.Translations.Any())
                {
                    _logger.LogDebug("Brand {BrandId} already has translations, skipping", brand.Id);
                    continue;
                }

                // Translate Name
                var nameTranslations = await _translationService.TranslateAsync(
                    brand.Name, "fr", TargetLanguages);

                // Translate Description (if exists)
                Dictionary<string, string>? descriptionTranslations = null;
                if (!string.IsNullOrWhiteSpace(brand.Description))
                {
                    descriptionTranslations = await _translationService.TranslateAsync(
                        brand.Description, "fr", TargetLanguages);
                }

                // Translate AboutUs (if exists)
                Dictionary<string, string>? aboutUsTranslations = null;
                if (!string.IsNullOrWhiteSpace(brand.AboutUs))
                {
                    aboutUsTranslations = await _translationService.TranslateAsync(
                        brand.AboutUs, "fr", TargetLanguages);
                }

                // Translate WhereAreWe (if exists)
                Dictionary<string, string>? whereAreWeTranslations = null;
                if (!string.IsNullOrWhiteSpace(brand.WhereAreWe))
                {
                    whereAreWeTranslations = await _translationService.TranslateAsync(
                        brand.WhereAreWe, "fr", TargetLanguages);
                }

                // Translate OtherInfo (if exists)
                Dictionary<string, string>? otherInfoTranslations = null;
                if (!string.IsNullOrWhiteSpace(brand.OtherInfo))
                {
                    otherInfoTranslations = await _translationService.TranslateAsync(
                        brand.OtherInfo, "fr", TargetLanguages);
                }

                // Add translations
                foreach (var lang in TargetLanguages)
                {
                    brand.AddOrUpdateTranslation(
                        lang,
                        nameTranslations[lang],
                        descriptionTranslations?[lang],
                        aboutUsTranslations?[lang],
                        whereAreWeTranslations?[lang],
                        otherInfoTranslations?[lang]);
                }

                _logger.LogInformation("Translated Brand {BrandId}: {BrandName}", brand.Id, brand.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate Brand {BrandId}: {BrandName}", brand.Id, brand.Name);
            }
        }
    }

    private async Task TranslateCategoriesAsync(bool dryRun)
    {
        _logger.LogInformation("--- Translating Categories ---");

        var categories = await _context.Categories
            .Include(c => c.Translations)
            .ToListAsync();

        _logger.LogInformation("Found {Count} categories to translate", categories.Count);

        foreach (var category in categories)
        {
            try
            {
                if (category.Translations.Any())
                {
                    _logger.LogDebug("Category {CategoryId} already has translations, skipping", category.Id);
                    continue;
                }

                var nameTranslations = await _translationService.TranslateAsync(
                    category.Name, "fr", TargetLanguages);

                foreach (var lang in TargetLanguages)
                {
                    category.AddOrUpdateTranslation(lang, nameTranslations[lang]);
                }

                _logger.LogInformation("Translated Category {CategoryId}: {CategoryName}", category.Id, category.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate Category {CategoryId}: {CategoryName}", category.Id, category.Name);
            }
        }
    }

    private async Task TranslateProductsAsync(bool dryRun)
    {
        _logger.LogInformation("--- Translating Products ---");

        var products = await _context.Products
            .Include(p => p.Translations)
            .ToListAsync();

        _logger.LogInformation("Found {Count} products to translate", products.Count);

        foreach (var product in products)
        {
            try
            {
                if (product.Translations.Any())
                {
                    _logger.LogDebug("Product {ProductId} already has translations, skipping", product.Id);
                    continue;
                }

                var nameTranslations = await _translationService.TranslateAsync(
                    product.Name, "fr", TargetLanguages);

                var descriptionTranslations = await _translationService.TranslateAsync(
                    product.Description, "fr", TargetLanguages);

                foreach (var lang in TargetLanguages)
                {
                    product.AddOrUpdateTranslation(
                        lang,
                        nameTranslations[lang],
                        descriptionTranslations[lang]);
                }

                _logger.LogInformation("Translated Product {ProductId}: {ProductName}", product.Id, product.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate Product {ProductId}: {ProductName}", product.Id, product.Name);
            }
        }
    }

    private async Task TranslateSizesAsync(bool dryRun)
    {
        _logger.LogInformation("--- Translating Sizes ---");

        var sizes = await _context.Sizes
            .Include(s => s.Translations)
            .ToListAsync();

        _logger.LogInformation("Found {Count} sizes to translate", sizes.Count);

        foreach (var size in sizes)
        {
            try
            {
                if (size.Translations.Any())
                {
                    _logger.LogDebug("Size {SizeId} already has translations, skipping", size.Id);
                    continue;
                }

                var nameTranslations = await _translationService.TranslateAsync(
                    size.Name, "fr", TargetLanguages);

                foreach (var lang in TargetLanguages)
                {
                    size.AddOrUpdateTranslation(lang, nameTranslations[lang]);
                }

                _logger.LogInformation("Translated Size {SizeId}: {SizeName}", size.Id, size.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate Size {SizeId}: {SizeName}", size.Id, size.Name);
            }
        }
    }

    private async Task TranslateColorsAsync(bool dryRun)
    {
        _logger.LogInformation("--- Translating Colors ---");

        var colors = await _context.Colors
            .Include(c => c.Translations)
            .ToListAsync();

        _logger.LogInformation("Found {Count} colors to translate", colors.Count);

        foreach (var color in colors)
        {
            try
            {
                if (color.Translations.Any())
                {
                    _logger.LogDebug("Color {ColorId} already has translations, skipping", color.Id);
                    continue;
                }

                var nameTranslations = await _translationService.TranslateAsync(
                    color.Name, "fr", TargetLanguages);

                foreach (var lang in TargetLanguages)
                {
                    color.AddOrUpdateTranslation(lang, nameTranslations[lang]);
                }

                _logger.LogInformation("Translated Color {ColorId}: {ColorName}", color.Id, color.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate Color {ColorId}: {ColorName}", color.Id, color.Name);
            }
        }
    }
}
