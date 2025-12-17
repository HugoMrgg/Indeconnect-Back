namespace IndeConnect_Back.Domain.catalog.brand;

public class BrandEthicScore
{
    public long Id { get; private set; }
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    public long CategoryId { get; private set; }
    public EthicsCategoryEntity Category { get; private set; } = default!;

    public long QuestionnaireId { get; private set; }
    public BrandQuestionnaire Questionnaire { get; private set; } = default!;

    public decimal RawScore { get; private set; }
    public decimal FinalScore { get; private set; }

    public bool IsOfficial { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private BrandEthicScore() { }

    public BrandEthicScore(long brandId, long categoryId, long questionnaireId, decimal rawScore, decimal finalScore, bool isOfficial)
    {
        BrandId = brandId;
        CategoryId = categoryId;
        QuestionnaireId = questionnaireId;
        RawScore = rawScore;
        FinalScore = finalScore;
        IsOfficial = isOfficial;
    }

    public void MarkOfficial() => IsOfficial = true;
    public void MarkNonOfficial() => IsOfficial = false;
}