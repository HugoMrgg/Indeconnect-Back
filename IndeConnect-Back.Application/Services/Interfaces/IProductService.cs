using IndeConnect_Back.Application.DTOs.Products;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IProductService
{
    Task<ProductDetailDto?> GetProductByIdAsync(long productId);
    Task<IEnumerable<ProductVariantDto>> GetProductVariantsAsync(long productId);
    Task<ProductStockDto?> GetProductStockAsync(long productId);
    Task<IEnumerable<ProductReviewDto>> GetProductReviewsAsync(long productId, int page = 1, int pageSize = 20);
    Task<ProductsListResponse> GetProductsByBrandAsync(GetProductsQuery query);
}