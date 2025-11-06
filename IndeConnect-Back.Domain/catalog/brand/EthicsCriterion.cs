namespace IndeConnect_Back.Domain.catalog.brand;

public class EthicsCriterion
{
    public long Id { get; private set; }
    public EthicsCategory Category { get; private set; }

    public string Key { get; private set; } = default!;   // ex: "material"
    public string Label { get; private set; } = default!; // ex: "Type de matière"
    public int Order { get; private set; }

    private readonly List<EthicsOption> _options = new();
    public IReadOnlyCollection<EthicsOption> Options => _options;

    private EthicsCriterion() { }

    public EthicsCriterion(EthicsCategory category, string key, string label, int order = 0)
    {
        Category = category;
        Key = key.Trim();
        Label = label.Trim();
        Order = order;
    }
}
