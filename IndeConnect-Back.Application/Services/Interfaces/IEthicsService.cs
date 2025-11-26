using IndeConnect_Back.Application.DTOs.Brands;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IEthicsService
{
    /// <summary>
    /// Récupère tous les tags éthiques disponibles avec le nombre de marques associées
    /// </summary>
    Task<EthicTagsListResponse> GetAllEthicTagsAsync();
}