using IndeConnect_Back.Application.DTOs.Products;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IProductService
{
    // Existing methods
    Task<ProductDetailDto?> GetProductByIdAsync(long productId);
    Task<IEnumerable<ProductVariantDto>> GetProductVariantsAsync(long productId);
    Task<ProductStockDto?> GetProductStockAsync(long productId);
    Task<IEnumerable<ProductReviewDto>> GetProductReviewsAsync(long productId, int page = 1, int pageSize = 20);
    Task<ProductsListResponse> GetProductsByBrandAsync(GetProductsQuery query);
    
    // Ajouter un nouveau produit
    Task<CreateProductResponse> CreateProductAsync(CreateProductQuery query);
    
    // Modifier un produit
    Task<UpdateProductResponse> UpdateProductAsync(UpdateProductQuery query);
    
    // NOUVEAUX : pour gérer les couleurs alternatives
    Task<IEnumerable<ProductColorVariantDto>> GetProductColorVariantsAsync(long productId);
    Task<ProductGroupDto?> GetProductGroupAsync(long productGroupId);
}