using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

/// <summary>
/// Service for resolving translations with fallback mechanism.
/// Language resolution priority: Query param → Header → Default (fr)
/// </summary>
public class TranslationService : ITranslationService
{
    private readonly ICurrentLanguageProvider _languageProvider;
    private readonly ILogger<TranslationService> _logger;
    private const string DefaultLanguage = "fr";

    public TranslationService(
        ICurrentLanguageProvider languageProvider,
        ILogger<TranslationService> logger)
    {
        _languageProvider = languageProvider;
        _logger = logger;
    }

    public string GetTranslatedValue<T>(
        IEnumerable<T> translations,
        string languageCode,
        Func<T, string?> selector,
        string fallbackValue) where T : class
    {
        var translationsList = translations?.ToList() ?? new List<T>();

        if (!translationsList.Any())
        {
            _logger.LogDebug("No translations available, returning fallback value");
            return fallbackValue;
        }

        // Try requested language
        var requestedTranslation = translationsList.FirstOrDefault(t =>
            GetLanguageCode(t) == languageCode.ToLower());

        if (requestedTranslation != null)
        {
            var value = selector(requestedTranslation);
            if (!string.IsNullOrWhiteSpace(value))
            {
                _logger.LogDebug("Found translation for language {Language}", languageCode);
                return value;
            }
        }

        // Fallback to French
        if (languageCode.ToLower() != DefaultLanguage)
        {
            var frenchTranslation = translationsList.FirstOrDefault(t =>
                GetLanguageCode(t) == DefaultLanguage);

            if (frenchTranslation != null)
            {
                var value = selector(frenchTranslation);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _logger.LogDebug("Using French fallback for language {Language}", languageCode);
                    return value;
                }
            }
        }

        // Final fallback to original value
        _logger.LogDebug("No suitable translation found, using fallback value");
        return fallbackValue;
    }

    public string GetCurrentLanguage()
    {
        return _languageProvider.GetCurrentLanguage();
    }

    public void SetCurrentLanguage(string languageCode)
    {
        _languageProvider.SetCurrentLanguage(languageCode);
    }

    private string GetLanguageCode<T>(T translation)
    {
        var property = typeof(T).GetProperty("LanguageCode");
        return (property?.GetValue(translation) as string)?.ToLower() ?? DefaultLanguage;
    }
}
