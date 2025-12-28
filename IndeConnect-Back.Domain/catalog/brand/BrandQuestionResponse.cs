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
    public string QuestionKey { get; private set; } = default!;
    private BrandQuestionResponse() { }

    public BrandQuestionResponse(long questionnaireId, long questionId, string questionKey)
    {
        QuestionnaireId = questionnaireId;
        QuestionId = questionId;
        QuestionKey = questionKey?.Trim() ?? throw new ArgumentNullException(nameof(questionKey));
    }

    public void SetCalculatedScore(decimal score) => CalculatedScore = score;

    /// <summary>
    /// Calcule le score de cette réponse en sommant les scores des options sélectionnées.
    /// </summary>
    /// <param name="optionsById">Dictionnaire des options par ID</param>
    /// <param name="selectedOptionIds">IDs des options sélectionnées</param>
    /// <returns>Le score total calculé (0 si aucune option sélectionnée)</returns>
    public static decimal CalculateScore(
        IReadOnlyDictionary<long, EthicsOption> optionsById,
        IEnumerable<long> selectedOptionIds)
    {
        var optionIdsList = selectedOptionIds.ToList();

        if (optionIdsList.Count == 0)
            return 0m;

        return optionIdsList.Sum(id => optionsById[id].Score);
    }
}