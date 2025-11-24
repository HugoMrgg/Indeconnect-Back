namespace IndeConnect_Back.Application.DTOs.Brands;

/// <summary>
/// Réponse contenant tous les tags éthiques disponibles
/// </summary>
public record EthicTagsListResponse(
    IEnumerable<EthicTagDto> Tags
);