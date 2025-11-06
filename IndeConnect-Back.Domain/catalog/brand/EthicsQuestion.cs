namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * represents an Ethic's question, created my administrator.
 */
public class EthicsQuestion
{
    public long Id { get; private set; }
    public EthicsCategory Category { get; private set; }

    public string Key { get; private set; } = default!; 
    public string Label { get; private set; } = default!; 
    public int Order { get; private set; }

    private readonly List<EthicsOption> _options = new();
    public IReadOnlyCollection<EthicsOption> Options => _options;

    private EthicsQuestion() { }

    public EthicsQuestion(EthicsCategory category, string key, string label, int order = 0)
    {
        Category = category;
        Key = key.Trim();
        Label = label.Trim();
        Order = order;
    }
}
