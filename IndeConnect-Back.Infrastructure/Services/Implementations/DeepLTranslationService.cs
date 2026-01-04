using IndeConnect_Back.Application.Services.Interfaces;
using DeepL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

/// <summary>
/// DeepL implementation of automatic translation service.
/// Requires DEEPL_API_KEY environment variable or configuration.
/// </summary>
public class DeepLTranslationService : IAutoTranslationService
{
    private readonly Translator _translator;
    private readonly ILogger<DeepLTranslationService> _logger;
    private readonly Dictionary<string, string> _languageMapping = new()
    {
        { "fr", "fr" },
        { "nl", "nl" },
        { "de", "de" },
        { "en", "en-GB" } // DeepL uses en-GB or en-US
    };

    public DeepLTranslationService(IConfiguration configuration, ILogger<DeepLTranslationService> logger)
    {
        _logger = logger;

        var apiKey = configuration["DeepL:ApiKey"]
                     ?? Environment.GetEnvironmentVariable("DEEPL_API_KEY")
                     ?? throw new InvalidOperationException(
                         "DeepL API key not found. Set DEEPL_API_KEY environment variable or DeepL:ApiKey in configuration.");

        _translator = new Translator(apiKey);
        _logger.LogInformation("DeepL Translator initialized successfully");
    }

    public async Task<Dictionary<string, string>> TranslateAsync(
        string text,
        string sourceLanguage,
        params string[] targetLanguages)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("Attempted to translate empty or null text");
            return targetLanguages.ToDictionary(lang => lang, _ => text);
        }

        var results = new Dictionary<string, string>();
        var sourceLang = MapLanguageCode(sourceLanguage);

        foreach (var targetLang in targetLanguages)
        {
            try
            {
                var translatedText = await TranslateSingleAsync(text, sourceLanguage, targetLang);
                results[targetLang] = translatedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate text to {TargetLanguage}. Using original text as fallback.", targetLang);
                results[targetLang] = text; // Fallback to original text
            }
        }

        return results;
    }

    public async Task<string> TranslateSingleAsync(
        string text,
        string sourceLanguage,
        string targetLanguage)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("Attempted to translate empty or null text");
            return text;
        }

        try
        {
            var sourceLang = MapLanguageCode(sourceLanguage);
            var targetLang = MapLanguageCode(targetLanguage);

            _logger.LogInformation("Translating from {SourceLang} to {TargetLang}", sourceLang, targetLang);

            var result = await _translator.TranslateTextAsync(
                text,
                sourceLang,
                targetLang);

            _logger.LogInformation("Translation successful: {OriginalLength} chars → {TranslatedLength} chars",
                text.Length, result.Text.Length);

            return result.Text;
        }
        catch (DeepLException ex)
        {
            _logger.LogError(ex, "DeepL API error while translating to {TargetLanguage}: {Message}",
                targetLanguage, ex.Message);
            throw new InvalidOperationException(
                $"Translation failed for language '{targetLanguage}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during translation to {TargetLanguage}", targetLanguage);
            throw;
        }
    }

    private string MapLanguageCode(string languageCode)
    {
        if (_languageMapping.TryGetValue(languageCode.ToLower(), out var mapped))
        {
            return mapped;
        }

        _logger.LogWarning("Language code '{LanguageCode}' not mapped, using as-is", languageCode);
        return languageCode;
    }
}
