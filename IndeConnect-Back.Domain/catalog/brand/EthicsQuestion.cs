namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * represents an Ethic's question, created my administrator.
 */
public class EthicsQuestion
{
    public long Id { get; private set; }
    public long CatalogVersionId { get; private set; }
    public CatalogVersion CatalogVersion { get; private set; } = default!;
    public EthicsCategory Category { get; private set; }
    public string Key { get; private set; } = default!;
    public string Label { get; private set; } = default!;
    public int Order { get; private set; }
    public EthicsAnswerType AnswerType { get; private set; } = EthicsAnswerType.Single;
    public bool IsActive { get; private set; } = true;

    private readonly List<EthicsOption> _options = new();
    public IReadOnlyCollection<EthicsOption> Options => _options;

    private EthicsQuestion() { }

    public EthicsQuestion(long catalogVersionId, EthicsCategory category, string key, string label, EthicsAnswerType answerType, int order = 0, bool isActive = true)
    {
        CatalogVersionId = catalogVersionId;
        Category = category;
        Key = key.Trim();
        Label = label.Trim();
        Order = order;
        AnswerType = answerType;
        IsActive = isActive;
    }

    /// <summary>
    /// Valide que les options sélectionnées respectent les contraintes du type de réponse.
    /// </summary>
    /// <param name="selectedOptionIds">IDs des options sélectionnées</param>
    /// <param name="isSubmitting">Indique si c'est une soumission finale (règles plus strictes)</param>
    /// <exception cref="InvalidOperationException">Si les contraintes ne sont pas respectées</exception>
    public void ValidateAnswerOptions(IEnumerable<long> selectedOptionIds, bool isSubmitting = false)
    {
        var optionIdsList = selectedOptionIds.Distinct().ToList();

        if (AnswerType == EthicsAnswerType.Single)
        {
            if (optionIdsList.Count > 1)
                throw new InvalidOperationException($"Question {Id} (Single): only one option allowed.");

            if (isSubmitting && optionIdsList.Count != 1)
                throw new InvalidOperationException($"Question {Id} (Single): exactly 1 option is required on submission.");
        }
        else if (AnswerType == EthicsAnswerType.Multiple)
        {
            if (isSubmitting && optionIdsList.Count < 1)
                throw new InvalidOperationException($"Question {Id} (Multiple): at least 1 option is required on submission.");
        }
        else
        {
            throw new InvalidOperationException($"Unsupported answer type for question {Id}.");
        }
    }

    /// <summary>
    /// Vérifie qu'une option appartient à cette question.
    /// </summary>
    /// <param name="optionId">L'ID de l'option à vérifier</param>
    /// <returns>True si l'option appartient à cette question</returns>
    public bool ContainsOption(long optionId)
    {
        return _options.Any(o => o.Id == optionId);
    }

    /// <summary>
    /// Valide que toutes les options sélectionnées appartiennent à cette question.
    /// </summary>
    /// <param name="selectedOptionIds">IDs des options sélectionnées</param>
    /// <param name="availableOptionsById">Dictionnaire des options disponibles</param>
    /// <exception cref="InvalidOperationException">Si une option n'existe pas ou n'appartient pas à la question</exception>
    public void ValidateOptionsOwnership(
        IEnumerable<long> selectedOptionIds,
        IReadOnlyDictionary<long, EthicsOption> availableOptionsById)
    {
        foreach (var optId in selectedOptionIds)
        {
            if (!availableOptionsById.TryGetValue(optId, out var option))
                throw new InvalidOperationException($"Unknown or inactive option: {optId}");

            if (option.QuestionId != Id)
                throw new InvalidOperationException($"Option {optId} does not belong to question {Id}.");
        }
    }
}
