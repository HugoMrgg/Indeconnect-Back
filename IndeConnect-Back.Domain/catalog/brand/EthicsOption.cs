namespace IndeConnect_Back.Domain.catalog.brand;

public class EthicsOption
{
    public long Id { get; private set; }
    public long CriterionId { get; private set; }
    public EthicsCriterion Criterion { get; private set; } = default!;

    public string Key { get; private set; } = default!;    // ex: "wool", "cotton", "ship", "plane"
    public string Label { get; private set; } = default!;
    public decimal Score { get; private set; }             // 0..1 (ou pondération)

    private EthicsOption() { }

    public EthicsOption(long criterionId, string key, string label, decimal score)
    {
        CriterionId = criterionId;
        Key = key.Trim();
        Label = label.Trim();
        Score = score;
    }
}