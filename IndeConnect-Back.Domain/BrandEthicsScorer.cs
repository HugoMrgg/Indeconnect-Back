using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Domain;

public class BrandEthicsScorer
{
    public decimal ComputeScore(
        IEnumerable<BrandQuestionResponse> responses,
        EthicsCategory category)
    {
        // Filtre : récupère juste les réponses dans la bonne catégorie
        var validResponses = responses
            .Where(r => r.Question.Category == category);

        // Additionne les scores des options choisies (pondérées !)
        decimal totalScore = 0;
        foreach (var response in validResponses)
        {
            totalScore += response.Option.Score;
        }
        return totalScore;
    }
}