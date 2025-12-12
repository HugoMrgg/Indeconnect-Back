using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain;

public class BrandEthicsScorer
{
    public decimal ComputeScore(
        IEnumerable<BrandQuestionResponse> responses,
        EthicsCategory category)
    {
        if (responses == null)
            return 0m;

        var validResponses = responses
            .Where(r => r != null
                        && r.Question != null
                        && r.Option != null
                        && r.Question.Category == category);

        decimal totalScore = 0m;
        foreach (var response in validResponses)
        {
            totalScore += response.Option.Score;
        }

        return totalScore;
    }

}