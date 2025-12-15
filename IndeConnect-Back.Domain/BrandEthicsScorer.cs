using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain;

public class BrandEthicsScorer
{
    public decimal ComputeRawScore(IEnumerable<BrandQuestionResponse> responses, long categoryId)
    {
        if (responses == null)
            return 0m;

        var validResponses = responses
            .Where(r => r?.Question != null 
                        && r.Question.CategoryId == categoryId);

        decimal totalScore = 0m;
        foreach (var r in validResponses)
            totalScore += r.SelectedOptions.Sum(so => so.Option.Score);

        return totalScore;
    }

}