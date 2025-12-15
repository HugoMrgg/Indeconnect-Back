namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * represents an Ethic's question, created my administrator.
 */
public class EthicsQuestion
{
    public long Id { get; private set; }
    public long CategoryId { get; private set; }
    public EthicsCategoryEntity Category { get; private set; } = default!;
    public string Key { get; private set; } = default!; 
    public string Label { get; private set; } = default!; 
    public int Order { get; private set; }
    public EthicsAnswerType AnswerType { get; private set; } = EthicsAnswerType.Single;
    public bool IsActive { get; private set; } = true;

    private readonly List<EthicsOption> _options = new();
    public IReadOnlyCollection<EthicsOption> Options => _options;

    private EthicsQuestion() { }

    public EthicsQuestion(long categoryId, string key, string label, EthicsAnswerType answerType, int order = 0, bool isActive = true)
    {
        CategoryId = categoryId;
        Key = key.Trim();
        Label = label.Trim();
        Order = order;
        AnswerType = answerType;
        IsActive = isActive;
    }
}
