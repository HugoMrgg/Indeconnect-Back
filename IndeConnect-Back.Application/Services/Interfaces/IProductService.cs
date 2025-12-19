using IndeConnect_Back.Application.DTOs.Products;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IProductService
{
    // Existing methods
    Task<ProductDetailDto?> GetProductByIdAsync(long productId);
    Task<IEnumerable<ProductVariantDto>> GetProductVariantsAsync(long productId);
    Task<ProductStockDto?> GetProductStockAsync(long productId);
    
    // Reviews publics (uniquement Approved)
    Task<IEnumerable<ProductReviewDto>> GetProductReviewsAsync(long productId, int page = 1, int pageSize = 20);
    Task<ProductsListResponse> GetProductsByBrandAsync(GetProductsQuery query, long? userId);
    
    // Ajouter un nouveau produit
    Task<CreateProductResponse> CreateProductAsync(CreateProductQuery query, long? currentUserId);
    
    // Modifier un produit
    Task<UpdateProductResponse> UpdateProductAsync(long productId, UpdateProductQuery query, long? currentUserId);
    
    // NOUVEAUX : pour gérer les couleurs alternatives
    Task<IEnumerable<ProductColorVariantDto>> GetProductColorVariantsAsync(long productId);
    Task<ProductGroupDto?> GetProductGroupAsync(long productGroupId);
    
    // Création d’un review par un client
    Task<ProductReviewDto> AddProductReviewAsync(long productId, long userId, int rating, string? comment);

    // Pour le vendeur : voir toutes les reviews (tous statuts)
    Task<IEnumerable<ProductReviewDto>> GetAllProductReviewsAsync(long productId);

    // Pour le vendeur : modération
    Task ApproveProductReviewAsync(long reviewId, long sellerUserId);
    Task RejectProductReviewAsync(long reviewId, long sellerUserId);
}