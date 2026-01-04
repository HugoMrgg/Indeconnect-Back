namespace IndeConnect_Back.Application.Services.Interfaces;

/// <summary>
/// Service for automatic translation of text using external translation APIs (e.g., DeepL).
/// </summary>
public interface IAutoTranslationService
{
    /// <summary>
    /// Translates a text from source language to multiple target languages.
    /// </summary>
    /// <param name="text">The text to translate</param>
    /// <param name="sourceLanguage">Source language code (e.g., "fr")</param>
    /// <param name="targetLanguages">Array of target language codes (e.g., ["nl", "de", "en"])</param>
    /// <returns>Dictionary mapping language code to translated text</returns>
    Task<Dictionary<string, string>> TranslateAsync(
        string text,
        string sourceLanguage,
        params string[] targetLanguages);

    /// <summary>
    /// Translates a single text to a single target language.
    /// </summary>
    /// <param name="text">The text to translate</param>
    /// <param name="sourceLanguage">Source language code (e.g., "fr")</param>
    /// <param name="targetLanguage">Target language code (e.g., "nl")</param>
    /// <returns>Translated text</returns>
    Task<string> TranslateSingleAsync(
        string text,
        string sourceLanguage,
        string targetLanguage);
}
