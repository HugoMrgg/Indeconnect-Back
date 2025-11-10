using IndeConnect_Back.Application.DTOs.Products;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IProductService
{
    Task<ProductsListResponse> GetProductsByBrandAsync(GetProductsQuery query);
}