namespace IndeConnect_Back.Domain.catalog.brand;

public class BrandQuestionResponse
{
    public long Id { get; private set; }
    public long QuestionnaireId { get; private set; }
    public BrandQuestionnaire Questionnaire { get; private set; } = default!;

    public long CriterionId { get; private set; }
    public EthicsCriterion Criterion { get; private set; } = default!;

    public long OptionId { get; private set; }
    public EthicsOption Option { get; private set; } = default!;

    private BrandQuestionResponse() { }

    public BrandQuestionResponse(long questionnaireId, long criterionId, long optionId)
    {
        QuestionnaireId = questionnaireId;
        CriterionId = criterionId;
        OptionId = optionId;
    }
}