namespace IndeConnect_Back.Domain.catalog.product;

public class Color
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Hexa { get; private set; } = default!;

    // Translations
    private readonly List<ColorTranslation> _translations = new();
    public IReadOnlyCollection<ColorTranslation> Translations => _translations;

    private Color() { }

    public Color(string name, string hexa)
    {
        Name = name.Trim();
        Hexa = hexa.Trim();
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
            var translation = new ColorTranslation(Id, languageCode, name);
            _translations.Add(translation);
        }
    }

    public string GetTranslatedName(string languageCode = "fr")
    {
        var translation = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        return translation?.Name ?? _translations.FirstOrDefault(t => t.LanguageCode == "fr")?.Name ?? Name;
    }
}