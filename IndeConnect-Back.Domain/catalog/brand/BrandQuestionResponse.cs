namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * Represents a brand's questionnaire answer.
 */
public class BrandQuestionResponse
{
    public long Id { get; private set; }
    public long QuestionnaireId { get; private set; }
    public BrandQuestionnaire Questionnaire { get; private set; } = default!;

    public long QuestionId { get; private set; }
    public EthicsQuestion Question { get; private set; } = default!;

    public long OptionId { get; private set; }
    public EthicsOption Option { get; private set; } = default!;

    private BrandQuestionResponse() { }

    public BrandQuestionResponse(long questionnaireId, long questionId, long optionId)
    {
        QuestionnaireId = questionnaireId;
        QuestionId = questionId;
        OptionId = optionId;
    }
}