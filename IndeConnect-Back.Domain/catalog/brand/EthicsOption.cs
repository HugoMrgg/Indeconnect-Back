namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * Represents proposed answer to an ethic question.
 */
public class EthicsOption
{
    public long Id { get; private set; }
    public long QuestionId { get; private set; }
    public EthicsQuestion Question { get; private set; } = default!;

    public string Key { get; private set; } = default!; 
    public string Label { get; private set; } = default!;
    public decimal Score { get; private set; }       
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;

    private EthicsOption() { }

    public EthicsOption(long questionId, string key, string label, decimal score, int order = 0, bool isActive = true)
    {
        QuestionId = questionId;
        Key = key.Trim();
        Label = label.Trim();
        Score = score;
        Order = order;
        IsActive = isActive;
    }
}