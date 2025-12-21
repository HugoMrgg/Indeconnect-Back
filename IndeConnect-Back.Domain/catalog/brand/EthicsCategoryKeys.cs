namespace IndeConnect_Back.Domain.catalog.brand;

/// <summary>
/// Constantes métier pour les clés de catégories d'éthique.
/// Ces clés sont utilisées pour mapper les scores d'éthique aux bonnes catégories.
/// </summary>
public static class EthicsCategoryKeys
{
    /// <summary>
    /// Clés valides pour la catégorie "Production/Création".
    /// Inclut plusieurs alias pour robustesse (seed data peut varier).
    /// </summary>
    public static readonly string[] Production =
    {
        "materialsmanufacturing",
        "materials_manufacturing",
        "creation",
        "creation-des-habits",
        "production"
    };

    /// <summary>
    /// Clés valides pour la catégorie "Transport".
    /// </summary>
    public static readonly string[] Transport =
    {
        "transport"
    };

    /// <summary>
    /// Vérifie si une clé appartient à la catégorie Production.
    /// </summary>
    public static bool IsProductionKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return Production.Contains(key.ToLowerInvariant());
    }

    /// <summary>
    /// Vérifie si une clé appartient à la catégorie Transport.
    /// </summary>
    public static bool IsTransportKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return Transport.Contains(key.ToLowerInvariant());
    }
}
