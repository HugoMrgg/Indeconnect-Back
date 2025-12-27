namespace IndeConnect_Back.Domain.catalog.brand;

public class BrandQuestionResponseOption
{
    public long ResponseId { get; private set; }
    public BrandQuestionResponse Response { get; private set; } = default!;
    public long OptionId { get; private set; }
    public EthicsOption Option { get; private set; } = default!;

    private BrandQuestionResponseOption() { }

    public BrandQuestionResponseOption(long responseId, long optionId)
    {
        ResponseId = responseId;
        OptionId = optionId;
    }
}