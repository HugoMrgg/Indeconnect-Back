namespace IndeConnect_Back.Domain.catalog.product;

public class Size
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public int SortOrder { get; private set; }
    
    // Nouvelle relation vers Category
    public long CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;

    // Translations
    private readonly List<SizeTranslation> _translations = new();
    public IReadOnlyCollection<SizeTranslation> Translations => _translations;

    private Size() { }

    public Size(string name, long categoryId, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name.Trim();
        CategoryId = categoryId;
        SortOrder = sortOrder;
    }

    public void AddOrUpdateTranslation(string languageCode, string name)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);

        if (existing != null)
        {
            existing.UpdateTranslation(name);
        }
        else
        {
            var translation = new SizeTranslation(Id, languageCode, name);
            _translations.Add(translation);
        }
    }

    public string GetTranslatedName(string languageCode = "fr")
    {
        var translation = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        return translation?.Name ?? _translations.FirstOrDefault(t => t.LanguageCode == "fr")?.Name ?? Name;
    }
}