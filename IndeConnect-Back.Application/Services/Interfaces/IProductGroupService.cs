using IndeConnect_Back.Application.DTOs.Products;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IProductGroupService
{
    /// <summary>
    /// Crée un nouveau ProductGroup pour la marque du SuperVendor
    /// </summary>
    Task<ProductGroupDto> CreateProductGroupAsync(CreateProductGroupRequest request, long? currentUserId);

    /// <summary>
    /// Récupère un ProductGroup par son ID avec tous ses produits (variantes de couleur)
    /// </summary>
    Task<ProductGroupDto?> GetProductGroupByIdAsync(long productGroupId);

    /// <summary>
    /// Liste tous les ProductGroups d'une marque (pour le dropdown de sélection)
    /// </summary>
    Task<IEnumerable<ProductGroupSummaryDto>> GetProductGroupsByBrandAsync(long brandId);

    /// <summary>
    /// Met à jour un ProductGroup (nom, description, catégorie)
    /// </summary>
    Task<ProductGroupDto> UpdateProductGroupAsync(long productGroupId, UpdateProductGroupRequest request,
        long? currentUserId);
}