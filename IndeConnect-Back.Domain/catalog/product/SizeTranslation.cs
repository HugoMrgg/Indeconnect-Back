namespace IndeConnect_Back.Domain.catalog.product;

/// <summary>
/// Represents a translation of Size name in a specific language.
/// </summary>
public class SizeTranslation
{
    public long Id { get; private set; }
    public long SizeId { get; private set; }
    public Size Size { get; private set; } = default!;

    /// <summary>
    /// ISO 639-1 language code: "fr", "nl", "de", "en"
    /// </summary>
    public string LanguageCode { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    // EF Core constructor
    private SizeTranslation() { }

    public SizeTranslation(long sizeId, string languageCode, string name)
    {
        SizeId = sizeId;
        LanguageCode = languageCode?.ToLower().Trim() ?? throw new ArgumentNullException(nameof(languageCode));
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));

        ValidateLanguageCode();
    }

    public void UpdateTranslation(string name)
    {
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
    }

    private void ValidateLanguageCode()
    {
        var validCodes = new[] { "fr", "nl", "de", "en" };
        if (!validCodes.Contains(LanguageCode))
        {
            throw new ArgumentException(
                $"Invalid language code '{LanguageCode}'. Must be one of: {string.Join(", ", validCodes)}",
                nameof(LanguageCode));
        }
    }
}
