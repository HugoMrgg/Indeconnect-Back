namespace IndeConnect_Back.Application.Services.Interfaces;

/// <summary>
/// Provides the current request language from HTTP context.
/// This abstraction allows Infrastructure layer to access language without depending on ASP.NET Core HTTP.
/// </summary>
public interface ICurrentLanguageProvider
{
    /// <summary>
    /// Gets the current language code for the request.
    /// Priority: Query parameter (?lang=nl) → Accept-Language header → Default (fr)
    /// </summary>
    /// <returns>Language code: "fr", "nl", "de", or "en"</returns>
    string GetCurrentLanguage();

    /// <summary>
    /// Sets the current language for the request (stores in context items).
    /// </summary>
    /// <param name="languageCode">Language code to set</param>
    void SetCurrentLanguage(string languageCode);

    /// <summary>
    /// Checks if a language code is valid/supported.
    /// </summary>
    /// <param name="languageCode">Language code to validate</param>
    /// <returns>True if the language is supported</returns>
    bool IsValidLanguage(string languageCode);
}
