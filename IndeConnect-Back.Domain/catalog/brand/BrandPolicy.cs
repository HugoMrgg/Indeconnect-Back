namespace IndeConnect_Back.Domain.catalog.brand;

public class BrandPolicy
{
    public long Id { get; private set; }

    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    public PolicyType Type { get; private set; }       // Enum : Return, Delivery, CGV, Ethics, Privacy...
    public string Content { get; private set; } = default!; // Texte ou URL/rich text
    public string? Language { get; private set; }      // Pour international (fr, en, nl, etc.)

    public DateTime PublishedAt { get; private set; }
    public bool IsActive { get; private set; }

    private BrandPolicy() { } // EF

    public BrandPolicy(long brandId, PolicyType type, string content, string? language = null)
    {
        BrandId = brandId;
        Type = type;
        Content = content;
        Language = language;
        PublishedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    // Ajoute d'autres méthodes métier selon besoin
}

