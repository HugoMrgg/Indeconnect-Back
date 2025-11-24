namespace IndeConnect_Back.Application.DTOs.Brands;

/// <summary>
/// Représente un tag éthique disponible dans le système
/// </summary>
public record EthicTagDto(
    string Key,           // Clé technique (ex: "organic", "fair-trade")
    string Label,         // Label d'affichage (ex: "Bio", "Commerce équitable")
    string? Description,  // Description optionnelle
    int BrandCount        // Nombre de marques qui ont ce tag
);
