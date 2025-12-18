namespace IndeConnect_Back.Domain.catalog.brand;

/// <summary>
/// Service du domaine pour calculer le multiplicateur de distance appliqué au score d'éthique transport.
/// Plus la marque est proche de l'utilisateur, meilleur est le multiplicateur.
/// </summary>
public static class EthicsDistanceMultiplier
{
    /// <summary>
    /// Calcule le multiplicateur basé sur la distance entre l'utilisateur et la marque.
    /// - Moins de 50 km: multiplicateur 2.0 (très local)
    /// - Moins de 200 km: multiplicateur 1.5 (régional)
    /// - Moins de 500 km: multiplicateur 1.0 (national)
    /// - Plus de 500 km: multiplicateur 0.5 (international)
    /// </summary>
    /// <param name="distanceKm">La distance en kilomètres</param>
    /// <returns>Le multiplicateur à appliquer au score d'éthique transport</returns>
    public static double Calculate(double distanceKm)
    {
        return distanceKm switch
        {
            < 50 => 2.0,   // Très local (ville/commune)
            < 200 => 1.5,  // Régional (province/région)
            < 500 => 1.0,  // National
            _ => 0.5       // International ou lointain
        };
    }

    /// <summary>
    /// Applique le multiplicateur de distance au score de transport.
    /// Si la distance n'est pas fournie, retourne le score de base sans modification.
    /// </summary>
    /// <param name="transportBaseScore">Le score d'éthique transport de base</param>
    /// <param name="distanceKm">La distance en kilomètres (peut être null)</param>
    /// <returns>Le score ajusté avec le multiplicateur de distance</returns>
    public static double ApplyToScore(double transportBaseScore, double? distanceKm)
    {
        if (!distanceKm.HasValue)
            return transportBaseScore;

        var multiplier = Calculate(distanceKm.Value);
        return transportBaseScore * multiplier;
    }
}
