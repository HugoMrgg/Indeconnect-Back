namespace IndeConnect_Back.Domain.catalog.brand;

/// <summary>
/// Represents a translation of Brand text fields in a specific language.
/// Supports multilingual content for Brand Name, Description, AboutUs, WhereAreWe, and OtherInfo.
/// </summary>
public class BrandTranslation
{
    public long Id { get; private set; }
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    /// <summary>
    /// ISO 639-1 language code: "fr", "nl", "de", "en"
    /// </summary>
    public string LanguageCode { get; private set; } = default!;

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? AboutUs { get; private set; }
    public string? WhereAreWe { get; private set; }
    public string? OtherInfo { get; private set; }

    // EF Core constructor
    private BrandTranslation() { }

    public BrandTranslation(
        long brandId,
        string languageCode,
        string name,
        string? description = null,
        string? aboutUs = null,
        string? whereAreWe = null,
        string? otherInfo = null)
    {
        BrandId = brandId;
        LanguageCode = languageCode?.ToLower().Trim() ?? throw new ArgumentNullException(nameof(languageCode));
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Description = description?.Trim();
        AboutUs = aboutUs?.Trim();
        WhereAreWe = whereAreWe?.Trim();
        OtherInfo = otherInfo?.Trim();

        ValidateLanguageCode();
    }

    public void UpdateTranslation(
        string name,
        string? description = null,
        string? aboutUs = null,
        string? whereAreWe = null,
        string? otherInfo = null)
    {
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        Description = description?.Trim();
        AboutUs = aboutUs?.Trim();
        WhereAreWe = whereAreWe?.Trim();
        OtherInfo = otherInfo?.Trim();
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
