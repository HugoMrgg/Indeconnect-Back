namespace IndeConnect_Back.Domain.catalog.brand;

/// <summary>
/// Service du domaine responsable de la gestion du statut officiel des scores d'éthique.
/// Assure qu'une marque n'a qu'un seul set de scores "officiels" à la fois (le plus récent approuvé).
/// </summary>
public static class BrandEthicsScoreOfficializer
{
    /// <summary>
    /// Promouvoir les scores d'un questionnaire comme officiels et déclasser les anciens scores officiels.
    /// Cette méthode encapsule la règle métier : une marque ne peut avoir qu'UN set de scores officiels.
    /// </summary>
    /// <param name="allScores">Tous les scores de la marque</param>
    /// <param name="questionnaireId">L'ID du questionnaire dont les scores doivent être promus</param>
    /// <returns>Un tuple avec la liste des scores déclassés et la liste des scores promus</returns>
    public static (IEnumerable<BrandEthicScore> Demoted, IEnumerable<BrandEthicScore> Promoted) TransitionToOfficial(
        IEnumerable<BrandEthicScore> allScores,
        long questionnaireId)
    {
        var scoresList = allScores.ToList();
        var brandId = scoresList.FirstOrDefault()?.BrandId;

        if (brandId == null || !scoresList.Any())
            return (Enumerable.Empty<BrandEthicScore>(), Enumerable.Empty<BrandEthicScore>());

        // Filtrer les scores pour cette marque uniquement (sécurité)
        var brandScores = scoresList.Where(s => s.BrandId == brandId).ToList();

        // 1) Déclasser les anciens scores officiels
        var currentOfficials = brandScores
            .Where(s => s.IsOfficial)
            .ToList();

        foreach (var score in currentOfficials)
        {
            score.MarkOfficial();
        }

        // 2) Promouvoir les scores du questionnaire approuvé
        var scoresToPromote = brandScores
            .Where(s => s.QuestionnaireId == questionnaireId)
            .ToList();

        foreach (var score in scoresToPromote)
        {
            score.MarkOfficial();
        }

        return (currentOfficials, scoresToPromote);
    }

    /// <summary>
    /// Vérifie qu'une marque n'a qu'un seul set de scores officiels (par questionnaire).
    /// Utile pour les tests et validations.
    /// </summary>
    /// <param name="scores">Les scores à vérifier</param>
    /// <returns>True si la règle d'unicité est respectée</returns>
    public static bool ValidateOfficialScoreUniqueness(IEnumerable<BrandEthicScore> scores)
    {
        var officialScores = scores.Where(s => s.IsOfficial).ToList();

        // Si pas de scores officiels, c'est valide
        if (!officialScores.Any())
            return true;

        // Grouper par marque
        var scoresByBrand = officialScores.GroupBy(s => s.BrandId);

        // Pour chaque marque, vérifier qu'il n'y a qu'un seul questionnaire officiel
        foreach (var brandGroup in scoresByBrand)
        {
            var uniqueQuestionnaires = brandGroup.Select(s => s.QuestionnaireId).Distinct().Count();
            if (uniqueQuestionnaires > 1)
                return false; // Plus d'un questionnaire officiel pour cette marque
        }

        return true;
    }
}
