namespace IndeConnect_Back.Domain.catalog.brand;

/// <summary>
/// Represents a version of the ethics catalog.
/// Each version is immutable once published and contains a snapshot of questions and options at a point in time.
/// </summary>
public class CatalogVersion
{
    public long Id { get; private set; }
    public string VersionNumber { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDraft { get; private set; }

    private readonly List<EthicsQuestion> _questions = new();
    public IReadOnlyCollection<EthicsQuestion> Questions => _questions;

    private readonly List<BrandQuestionnaire> _questionnaires = new();
    public IReadOnlyCollection<BrandQuestionnaire> Questionnaires => _questionnaires;

    private CatalogVersion() { }

    public CatalogVersion(string versionNumber)
    {
        VersionNumber = versionNumber;
        CreatedAt = DateTimeOffset.UtcNow;
        IsDraft = true;
        IsActive = false;
    }

    public void Publish()
    {
        if (!IsDraft)
            throw new InvalidOperationException("Cannot publish a version that is not a draft.");

        IsDraft = false;
        IsActive = true;
        PublishedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (IsDraft)
            throw new InvalidOperationException("Cannot deactivate a draft version.");

        IsActive = false;
    }
}
