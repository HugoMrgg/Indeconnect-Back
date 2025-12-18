namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * represents an Ethic's question, created my administrator.
 */
public class EthicsQuestion
{
    public long Id { get; private set; }
    public long CatalogVersionId { get; private set; }
    public CatalogVersion CatalogVersion { get; private set; } = default!;
    public EthicsCategory Category { get; private set; }
    public string Key { get; private set; } = default!;
    public string Label { get; private set; } = default!;
    public int Order { get; private set; }
    public EthicsAnswerType AnswerType { get; private set; } = EthicsAnswerType.Single;
    public bool IsActive { get; private set; } = true;

    private readonly List<EthicsOption> _options = new();
    public IReadOnlyCollection<EthicsOption> Options => _options;

    private EthicsQuestion() { }

    public EthicsQuestion(long catalogVersionId, EthicsCategory category, string key, string label, EthicsAnswerType answerType, int order = 0, bool isActive = true)
    {
        CatalogVersionId = catalogVersionId;
        Category = category;
        Key = key.Trim();
        Label = label.Trim();
        Order = order;
        AnswerType = answerType;
        IsActive = isActive;
    }
}
