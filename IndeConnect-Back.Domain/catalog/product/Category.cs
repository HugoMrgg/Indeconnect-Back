namespace IndeConnect_Back.Domain.catalog.product;

public class Category
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    
    // Nouvelle collection de tailles
    private readonly List<Size> _sizes = new();
    public IReadOnlyCollection<Size> Sizes => _sizes;

    // Translations
    private readonly List<CategoryTranslation> _translations = new();
    public IReadOnlyCollection<CategoryTranslation> Translations => _translations;

    private Category() { }

    public Category(string name) => Name = name.Trim();

    public void AddOrUpdateTranslation(string languageCode, string name)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);

        if (existing != null)
        {
            existing.UpdateTranslation(name);
        }
        else
        {
            var translation = new CategoryTranslation(Id, languageCode, name);
            _translations.Add(translation);
        }
    }

    public string GetTranslatedName(string languageCode = "fr")
    {
        var translation = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        return translation?.Name ?? _translations.FirstOrDefault(t => t.LanguageCode == "fr")?.Name ?? Name;
    }
}