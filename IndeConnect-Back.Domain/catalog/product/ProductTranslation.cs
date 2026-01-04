namespace IndeConnect_Back.Domain.catalog.product;

/// <summary>
/// Represents a translation of Product text fields in a specific language.
/// Supports multilingual content for Product Name and Description.
/// </summary>
public class ProductTranslation
{
    public long Id { get; private set; }
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    /// <summary>
    /// ISO 639-1 language code: "fr", "nl", "de", "en"
    /// </summary>
    public string LanguageCode { get; private set; } = default!;

    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    // EF Core constructor
    private ProductTranslation() { }

    public ProductTranslation(
        long productId,
        string languageCode,
        string name,
        string description)
    {
        ProductId = productId;
        LanguageCode = languageCode?.ToLower().Trim() ?? throw new ArgumentNullException(nameof(languageCode));
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Description = description?.Trim() ?? throw new ArgumentNullException(nameof(description));

        ValidateLanguageCode();
    }

    public void UpdateTranslation(string name, string description)
    {
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Description = description?.Trim() ?? throw new ArgumentNullException(nameof(description));
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
