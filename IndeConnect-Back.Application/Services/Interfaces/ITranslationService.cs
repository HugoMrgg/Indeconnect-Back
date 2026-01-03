namespace IndeConnect_Back.Application.Services.Interfaces;

/// <summary>
/// Service for resolving translated content with fallback to French.
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// Gets the translated value from a collection of translations, with fallback to French and then original value.
    /// </summary>
    /// <typeparam name="T">Translation entity type</typeparam>
    /// <param name="translations">Collection of translations</param>
    /// <param name="languageCode">Requested language code</param>
    /// <param name="selector">Function to select the desired property from the translation</param>
    /// <param name="fallbackValue">Fallback value if no translation is found</param>
    /// <returns>Translated value or fallback</returns>
    string GetTranslatedValue<T>(
        IEnumerable<T> translations,
        string languageCode,
        Func<T, string?> selector,
        string fallbackValue) where T : class;

    /// <summary>
    /// Gets the current language code from the HTTP context (from header or query parameter).
    /// </summary>
    /// <returns>Language code (fr, nl, de, or en). Defaults to "fr".</returns>
    string GetCurrentLanguage();

    /// <summary>
    /// Sets the current language for the request.
    /// </summary>
    /// <param name="languageCode">Language code to set</param>
    void SetCurrentLanguage(string languageCode);
}
