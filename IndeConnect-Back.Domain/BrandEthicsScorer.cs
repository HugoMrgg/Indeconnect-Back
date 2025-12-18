using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain;

public class BrandEthicsScorer
{
    public decimal ComputeRawScore(IEnumerable<BrandQuestionResponse> responses, EthicsCategory category)
    {
        if (responses == null)
            return 0m;

        var validResponses = responses
            .Where(r => r?.Question != null
                        && r.Question.Category == category);

        decimal totalScore = 0m;
        foreach (var r in validResponses)
            totalScore += r.SelectedOptions.Sum(so => so.Option.Score);

        return totalScore;
    }

    public decimal ComputeFinalScore(decimal rawScore, decimal maxPossibleScore)
    {
        if (maxPossibleScore <= 0)
            return 0m;

        // Convert to percentage (0-100)
        return Math.Round((rawScore / maxPossibleScore) * 100m, 2);
    }

}