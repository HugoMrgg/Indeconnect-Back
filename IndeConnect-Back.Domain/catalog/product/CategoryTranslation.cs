namespace IndeConnect_Back.Domain.catalog.product;

/// <summary>
/// Represents a translation of Category name in a specific language.
/// </summary>
public class CategoryTranslation
{
    public long Id { get; private set; }
    public long CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;

    /// <summary>
    /// ISO 639-1 language code: "fr", "nl", "de", "en"
    /// </summary>
    public string LanguageCode { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    // EF Core constructor
    private CategoryTranslation() { }

    public CategoryTranslation(long categoryId, string languageCode, string name)
    {
        CategoryId = categoryId;
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
