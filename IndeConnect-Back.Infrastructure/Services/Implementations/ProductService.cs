using IndeConnect_Back.Application.DTOs.Products;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.catalog.product;
using IndeConnect_Back.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductsListResponse> GetProductsByBrandAsync(GetProductsQuery query)
    {
        // Vérifier que la marque existe et est approuvée
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == query.BrandId && b.Status == BrandStatus.Approved);

        if (brand == null)
            throw new InvalidOperationException("Brand not found or not approved");

        // Construire la requête produits
        var productsQuery = _context.Products
            .Where(p => p.BrandId == query.BrandId && p.Status == ProductStatus.Online) // ✅ Active, pas Draft !
            .Include(p => p.Reviews)
            .Include(p => p.Variants) // ✅ Charger les variants
                .ThenInclude(v => v.Media) // ✅ Charger les médias des variants
            .Include(p => p.Category) // ✅ Charger la catégorie pour le filtre
            .AsQueryable();

        // Filtre par catégorie
        if (!string.IsNullOrEmpty(query.Category))
        {
            productsQuery = productsQuery.Where(p => p.Category.Name == query.Category);
        }

        // Filtre par gamme de prix
        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);
        }

        // Filtre par terme de recherche
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            productsQuery = productsQuery.Where(p => 
                p.Name.ToLower().Contains(searchTerm) || 
                p.Description.ToLower().Contains(searchTerm)
            );
        }

        // Récupérer tous les résultats avant tri (nécessaire pour calculer les ratings)
        var products = await productsQuery.ToListAsync();

        // Mapper et calculer les ratings
        var enrichedProducts = products
            .Select(p => new
            {
                Product = p,
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => (double)r.Rating) : 0.0
            })
            .ToList();

        // Tri
        var sortedProducts = query.SortBy switch
        {
            ProductSortType.PriceAsc => enrichedProducts.OrderBy(x => x.Product.Price).ToList(),
            ProductSortType.PriceDesc => enrichedProducts.OrderByDescending(x => x.Product.Price).ToList(),
            ProductSortType.Rating => enrichedProducts.OrderByDescending(x => x.AverageRating).ToList(),
            ProductSortType.Popular => enrichedProducts.OrderByDescending(x => x.Product.Reviews.Count).ToList(),
            ProductSortType.Newest => enrichedProducts.OrderByDescending(x => x.Product.CreatedAt).ToList(),
            _ => enrichedProducts.OrderByDescending(x => x.Product.CreatedAt).ToList()
        };

        var totalCount = sortedProducts.Count;

        // Pagination
        var paginatedProducts = sortedProducts
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => MapToProductSummary(x.Product, x.AverageRating))
            .ToList();

        return new ProductsListResponse(
            paginatedProducts,
            totalCount,
            query.Page,
            query.PageSize
        );
    }

    private ProductSummaryDto MapToProductSummary(Product product, double averageRating)
    {
        // ✅ Récupérer l'image principale du premier variant (ou la première image disponible)
        var primaryImage = product.Variants
            .SelectMany(v => v.Media)
            .Where(m => m.IsPrimary)
            .OrderBy(m => m.DisplayOrder)
            .FirstOrDefault()?.Url;

        // Si pas d'image primaire, prendre la première image disponible
        if (primaryImage == null)
        {
            primaryImage = product.Variants
                .SelectMany(v => v.Media)
                .OrderBy(m => m.DisplayOrder)
                .FirstOrDefault()?.Url;
        }

        return new ProductSummaryDto(
            product.Id,
            product.Name,
            primaryImage, // ✅ Image du variant
            product.Price,
            product.Description,
            Math.Round(averageRating, 1),
            product.Reviews.Count
        );
    }
}
