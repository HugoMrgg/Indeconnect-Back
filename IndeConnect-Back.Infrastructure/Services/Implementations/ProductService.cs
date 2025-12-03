using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.DTOs.Products;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.catalog.product;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    /**
     * Retrieves detailed product information including variants, reviews, stock, and color alternatives
     */
    public async Task<ProductDetailDto?> GetProductByIdAsync(long productId)
    {
        // Load product with all related entities
        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Sale)
            .Include(p => p.PrimaryColor) // NOUVEAU
            .Include(p => p.Media) // NOUVEAU : médias sur Product directement
            .Include(p => p.ProductGroup) // NOUVEAU
                .ThenInclude(pg => pg.Products) // NOUVEAU : autres couleurs du groupe
                    .ThenInclude(p => p.PrimaryColor)
            .Include(p => p.ProductGroup)
                .ThenInclude(pg => pg.Products)
                    .ThenInclude(p => p.Media)
            .Include(p => p.Variants) // Variants = juste les tailles maintenant
                .ThenInclude(v => v.Size)
            .Include(p => p.Details)
            .Include(p => p.Keywords)
                .ThenInclude(pk => pk.Keyword)
            .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Approved))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsEnabled);

        if (product == null)
            return null;

        // Calculate sale price
        var basePrice = product.Price;
        decimal? salePrice = null;
        
        if (product.Sale != null && product.Sale.IsActive 
            && product.Sale.StartDate <= DateTimeOffset.UtcNow 
            && product.Sale.EndDate >= DateTimeOffset.UtcNow)
        {
            salePrice = basePrice * (1 - product.Sale.DiscountPercentage / 100);
        }

        // Calculate total stock
        var totalStock = product.Variants.Sum(v => v.StockCount);
        var isAvailable = totalStock > 0 && product.Status == ProductStatus.Online;

        // Calculate average rating
        var approvedReviews = product.Reviews.Where(r => r.Status == ReviewStatus.Approved).ToList();
        var avgRating = approvedReviews.Any() ? approvedReviews.Average(r => (double)r.Rating) : 0.0;

        // NOUVEAU : Récupérer les autres couleurs disponibles
        var colorVariants = product.ProductGroup?.Products
            .Where(p => p.Id != productId && p.IsEnabled && p.Status == ProductStatus.Online)
            .Select(p => new ProductColorVariantDto(
                p.Id,
                p.PrimaryColor?.Id,
                p.PrimaryColor?.Name,
                p.PrimaryColor?.Hexa,
                p.Media.FirstOrDefault(m => m.IsPrimary)?.Url 
                    ?? p.Media.OrderBy(m => m.DisplayOrder).FirstOrDefault()?.Url,
                p.Variants.Sum(v => v.StockCount) > 0
            ))
            .ToList() ?? new List<ProductColorVariantDto>();

        return new ProductDetailDto(
            product.Id,
            product.Name,
            product.Description,
            basePrice,
            salePrice,
            product.Sale != null ? MapToSaleDto(product.Sale) : null,
            new BrandSummaryDto(
                product.Brand.Id,
                product.Brand.Name,
                product.Brand.LogoUrl,
                product.Brand.BannerUrl,
                product.Brand.Description,
                0,          
                0, 
                Enumerable.Empty<string>(),
                product.Brand.Deposits.FirstOrDefault() != null 
                    ? $"{product.Brand.Deposits.First().Number} {product.Brand.Deposits.First().Street}, {product.Brand.Deposits.First().PostalCode}" 
                    : null,    
                null,       
                0             
            ),
            new CategoryDto(product.Category.Id, product.Category.Name),
            product.PrimaryColor != null 
                ? new ColorDto(product.PrimaryColor.Id, product.PrimaryColor.Name, product.PrimaryColor.Hexa) 
                : null, // NOUVEAU : couleur principale de ce produit
            colorVariants, // NOUVEAU : autres couleurs disponibles
            product.Media.OrderBy(m => m.DisplayOrder).Select(m => new ProductMediaDto(
                m.Id,
                m.Url,
                m.Type,
                m.DisplayOrder,
                m.IsPrimary
            )), // NOUVEAU : médias du produit
            product.Variants.Select(MapToVariantDto), // Maintenant juste les tailles
            product.Details.OrderBy(d => d.DisplayOrder).Select(d => new ProductDetailItemDto(d.Value, d.DisplayOrder)),
            product.Keywords.Select(pk => pk.Keyword.Name),
            approvedReviews.Select(MapToReviewDto),
            Math.Round(avgRating, 1),
            approvedReviews.Count,
            totalStock,
            product.Status,
            product.CreatedAt
        );
    }

    /**
     * Retrieves all size variants for a product
     */
    public async Task<IEnumerable<ProductVariantDto>> GetProductVariantsAsync(long productId)
    {
        var variants = await _context.ProductVariants
            .Include(v => v.Size)
            .Include(v => v.Product)
            .Where(v => v.ProductId == productId && v.Product.IsEnabled)
            .ToListAsync();

        return variants.Select(MapToVariantDto);
    }

    /**
     * NOUVEAU : Retrieves color variants (other products in the same ProductGroup)
     */
    public async Task<IEnumerable<ProductColorVariantDto>> GetProductColorVariantsAsync(long productId)
    {
        var product = await _context.Products
            .Include(p => p.ProductGroup)
                .ThenInclude(pg => pg.Products)
                    .ThenInclude(p => p.PrimaryColor)
            .Include(p => p.ProductGroup)
                .ThenInclude(pg => pg.Products)
                    .ThenInclude(p => p.Media)
            .Include(p => p.ProductGroup)
                .ThenInclude(pg => pg.Products)
                    .ThenInclude(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product?.ProductGroup == null)
            return Enumerable.Empty<ProductColorVariantDto>();

        return product.ProductGroup.Products
            .Where(p => p.IsEnabled && p.Status == ProductStatus.Online)
            .Select(p => new ProductColorVariantDto(
                p.Id,
                p.PrimaryColor?.Id,
                p.PrimaryColor?.Name,
                p.PrimaryColor?.Hexa,
                p.Media.FirstOrDefault(m => m.IsPrimary)?.Url 
                    ?? p.Media.OrderBy(m => m.DisplayOrder).FirstOrDefault()?.Url,
                p.Variants.Sum(v => v.StockCount) > 0
            ));
    }

    /**
     * NOUVEAU : Get ProductGroup details with all color variants
     */
    public async Task<ProductGroupDto?> GetProductGroupAsync(long productGroupId)
    {
        var group = await _context.ProductGroups
            .Include(pg => pg.Brand)
            .Include(pg => pg.Category)
            .Include(pg => pg.Products)
                .ThenInclude(p => p.PrimaryColor)
            .Include(pg => pg.Products)
                .ThenInclude(p => p.Media)
            .Include(pg => pg.Products)
                .ThenInclude(p => p.Variants)
            .FirstOrDefaultAsync(pg => pg.Id == productGroupId);

        if (group == null)
            return null;

        return new ProductGroupDto(
            group.Id,
            group.Name,
            group.BaseDescription,
            new BrandSummaryDto(
                group.Brand.Id,
                group.Brand.Name,
                group.Brand.LogoUrl,
                group.Brand.BannerUrl,
                group.Brand.Description,
                0, 0, null, null, null, 0
            ),
            new CategoryDto(group.Category.Id, group.Category.Name),
            group.Products
                .Where(p => p.IsEnabled && p.Status == ProductStatus.Online)
                .Select(p => new ProductColorVariantDto(
                    p.Id,
                    p.PrimaryColor?.Id,
                    p.PrimaryColor?.Name,
                    p.PrimaryColor?.Hexa,
                    p.Media.FirstOrDefault(m => m.IsPrimary)?.Url 
                        ?? p.Media.OrderBy(m => m.DisplayOrder).FirstOrDefault()?.Url,
                    p.Variants.Sum(v => v.StockCount) > 0
                ))
        );
    }

    /**
     * Retrieves stock information for a product and all its size variants
     */
    public async Task<ProductStockDto?> GetProductStockAsync(long productId)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsEnabled);

        if (product == null)
            return null;

        var totalStock = product.Variants.Sum(v => v.StockCount);
        var variantStocks = product.Variants.Select(v => new VariantStockDto(
            v.Id,
            v.SKU,
            v.StockCount,
            v.StockCount > 0
        ));

        return new ProductStockDto(
            productId,
            totalStock,
            totalStock > 0,
            variantStocks
        );
    }

    /**
     * Retrieves paginated approved reviews for a product
     */
    public async Task<IEnumerable<ProductReviewDto>> GetProductReviewsAsync(long productId, int page = 1, int pageSize = 20)
    {
        var reviews = await _context.ProductReviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return reviews.Select(MapToReviewDto);
    }

    /**
     * Retrieves paginated and filtered products for a specific brand
     * IMPORTANT: Returns individual products (each color = separate entry)
     */
    public async Task<ProductsListResponse> GetProductsByBrandAsync(GetProductsQuery query)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == query.BrandId && b.Status == BrandStatus.Approved);

        if (brand == null)
            throw new InvalidOperationException("Brand not found or not approved");

        // MODIFIÉ : Query sur Products directement (chaque couleur = un produit)
        var productsQuery = _context.Products
            .Where(p => p.BrandId == query.BrandId && p.Status == ProductStatus.Online) 
            .Include(p => p.Reviews)
            .Include(p => p.Variants) 
            .Include(p => p.Media) // NOUVEAU
            .Include(p => p.Category) 
            .Include(p => p.PrimaryColor) // NOUVEAU
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(query.Category))
        {
            productsQuery = productsQuery.Where(p => p.Category.Name == query.Category);
        }

        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);
        }

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            productsQuery = productsQuery.Where(p => 
                p.Name.ToLower().Contains(searchTerm) || 
                p.Description.ToLower().Contains(searchTerm)
            );
        }

        var products = await productsQuery.ToListAsync();

        var enrichedProducts = products
            .Select(p => new
            {
                Product = p,
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => (double)r.Rating) : 0.0
            })
            .ToList();

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

    /**
     * Create product for a brand
     */
    public async Task<CreateProductResponse> CreateProductAsync(CreateProductQuery query)
    {
        return new CreateProductResponse();
    }
    
    /**
    * Update product for a brand
    */
    public async Task<UpdateProductResponse> UpdateProductAsync(UpdateProductQuery query)
    {
        return new UpdateProductResponse();
    }


    /**
     * Maps a product variant (size) to DTO
     */
    private ProductVariantDto MapToVariantDto(ProductVariant variant)
    {
        var price = variant.PriceOverride ?? variant.Product.Price;
        
        if (variant.Product.Sale != null && variant.Product.Sale.IsActive
            && variant.Product.Sale.StartDate <= DateTimeOffset.UtcNow
            && variant.Product.Sale.EndDate >= DateTimeOffset.UtcNow)
        {
            price *= (1 - variant.Product.Sale.DiscountPercentage / 100);
        }

        return new ProductVariantDto(
            variant.Id,
            variant.SKU,
            variant.Size != null ? new SizeDto(variant.Size.Id, variant.Size.Name) : null,
            variant.StockCount,
            price,
            variant.StockCount > 0
        );
    }

    /**
     * Maps a product to summary DTO (for listing pages)
     */
    private ProductSummaryDto MapToProductSummary(Product product, double averageRating)
    {
        // MODIFIÉ : Prend l'image primaire du Product directement
        var primaryImage = product.Media
            .Where(m => m.IsPrimary)
            .OrderBy(m => m.DisplayOrder)
            .FirstOrDefault()?.Url;

        if (primaryImage == null)
        {
            primaryImage = product.Media
                .OrderBy(m => m.DisplayOrder)
                .FirstOrDefault()?.Url;
        }

        return new ProductSummaryDto(
            product.Id,
            product.Name,
            primaryImage, 
            product.Price,
            product.Description,
            Math.Round(averageRating, 1),
            product.Reviews.Count,
            product.PrimaryColor != null 
                ? new ColorDto(product.PrimaryColor.Id, product.PrimaryColor.Name, product.PrimaryColor.Hexa)
                : null // NOUVEAU : affiche la couleur dans le listing
        );
    }

    private ProductReviewDto MapToReviewDto(ProductReview review)
    {
        return new ProductReviewDto(
            review.Id,
            review.UserId,
            $"{review.User.FirstName} {review.User.LastName}",
            review.Rating,
            review.Comment,
            review.CreatedAt,
            review.Status
        );
    }

    private SaleDto MapToSaleDto(Sale sale)
    {
        return new SaleDto(
            sale.Id,
            sale.Name,
            sale.Description,
            sale.DiscountPercentage,
            sale.StartDate,
            sale.EndDate,
            sale.IsActive
        );
    }
}
