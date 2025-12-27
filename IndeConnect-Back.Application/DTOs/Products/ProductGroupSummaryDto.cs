namespace IndeConnect_Back.Application.DTOs.Products;

/// <summary>
/// Résumé d'un ProductGroup pour les listes (sans charger tous les produits)
/// Utilisé pour le dropdown de sélection de groupe
/// </summary>
public record ProductGroupSummaryDto(
    long Id,
    string Name,
    string BaseDescription,
    long CategoryId,
    string CategoryName,
    int ProductCount
);
