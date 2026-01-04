using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain;

public class BrandEthicsScorer
{
    public decimal ComputeRawScore(IEnumerable<BrandQuestionResponse> responses, EthicsCategory category)
    {
        if (responses == null)
        {
            Console.WriteLine($"[DEBUG] ComputeRawScore: responses is null for category {category}");
            return 0m;
        }

        var responsesList = responses.ToList();
        Console.WriteLine($"[DEBUG] ComputeRawScore: Processing {responsesList.Count} responses for category {category}");

        foreach (var r in responsesList)
        {
            var questionCategory = r.Question?.Category;
            var selectedCount = r.SelectedOptions?.Count() ?? 0;
            Console.WriteLine($"[DEBUG] Response QuestionId={r.QuestionId}, Question.Category={questionCategory}, SelectedOptions.Count={selectedCount}");

            if (r.SelectedOptions != null)
            {
                foreach (var so in r.SelectedOptions)
                {
                    Console.WriteLine($"[DEBUG]   - OptionId={so.OptionId}, Option.Score={so.Option?.Score ?? 0}");
                }
            }
        }

        var validResponses = responsesList
            .Where(r => r != null && r.Question != null && r.Question.Category == category)
            .ToList();

        Console.WriteLine($"[DEBUG] Found {validResponses.Count} valid responses for category {category}");

        decimal totalScore = 0m;
        foreach (var r in validResponses)
        {
            var responseScore = r.SelectedOptions.Sum(so => so.Option.Score);
            Console.WriteLine($"[DEBUG] Response score for QuestionId={r.QuestionId}: {responseScore}");
            totalScore += responseScore;
        }

        Console.WriteLine($"[DEBUG] Total raw score for category {category}: {totalScore}");
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