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

    private readonly List<BrandQuestionResponseOption> _selectedOptions = new();
    public IReadOnlyCollection<BrandQuestionResponseOption> SelectedOptions => _selectedOptions;
    public decimal? CalculatedScore { get; private set; } 
    private BrandQuestionResponse() { }

    public BrandQuestionResponse(long questionnaireId, long questionId)
    {
        QuestionnaireId = questionnaireId;
        QuestionId = questionId;
    }
    
    public void SetCalculatedScore(decimal score) => CalculatedScore = score;
}