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
     * Retrieves detailed product information including variants, reviews, and stock
     */
    public async Task<ProductDetailDto?> GetProductByIdAsync(long productId)
    {
        // Load product with all related entities (brand, category, variants, reviews, etc.)
        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Sale)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Size)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Color)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Media)
            .Include(p => p.Details)
            .Include(p => p.Keywords)
                .ThenInclude(pk => pk.Keyword)
            .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Approved))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsEnabled);

        if (product == null)
            return null;

        // Calculate sale price if an active promotion exists
        var basePrice = product.Price;
        decimal? salePrice = null;
        
        if (product.Sale != null && product.Sale.IsActive 
            && product.Sale.StartDate <= DateTimeOffset.UtcNow 
            && product.Sale.EndDate >= DateTimeOffset.UtcNow)
        {
            salePrice = basePrice * (1 - product.Sale.DiscountPercentage / 100);
        }

        // Calculate total stock across all variants
        var totalStock = product.Variants.Sum(v => v.StockCount);
        var isAvailable = totalStock > 0 && product.Status == ProductStatus.Online;

        // Calculate average rating from approved reviews only
        var approvedReviews = product.Reviews.Where(r => r.Status == ReviewStatus.Approved).ToList();
        var avgRating = approvedReviews.Any() ? approvedReviews.Average(r => (double)r.Rating) : 0.0;

        // Map to DTO with all computed values
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
                product.Brand.Description,
                0,          
                0,       
                null,
                product.Brand.Deposits.FirstOrDefault() != null 
                    ? $"{product.Brand.Deposits.First().Number} {product.Brand.Deposits.First().Street}, {product.Brand.Deposits.First().PostalCode}" 
                    : null,    
                null,       
                0             
            ),

            new CategoryDto(product.Category.Id, product.Category.Name),
            product.Variants.Select(MapToVariantDto),
            product.Details.OrderBy(d => d.DisplayOrder).Select(d => new ProductDetailItemDto(d.Key, d.Value, d.DisplayOrder)),
            product.Keywords.Select(pk => pk.Keyword.Name),
            approvedReviews.Select(MapToReviewDto),
            Math.Round(avgRating, 1),
            approvedReviews.Count,
            totalStock,
            isAvailable,
            product.Status,
            product.CreatedAt
        );
    }

    /**
     * Retrieves all variants for a product with size, color, and media information
     */
    public async Task<IEnumerable<ProductVariantDto>> GetProductVariantsAsync(long productId)
    {
        // Load variants with related size, color, and media data
        var variants = await _context.ProductVariants
            .Include(v => v.Size)
            .Include(v => v.Color)
            .Include(v => v.Media)
            .Include(v => v.Product)
            .Where(v => v.ProductId == productId && v.Product.IsEnabled)
            .ToListAsync();

        return variants.Select(MapToVariantDto);
    }

    /**
     * Retrieves stock information for a product and all its variants
     */
    public async Task<ProductStockDto?> GetProductStockAsync(long productId)
    {
        // Load product with variants to calculate stock
        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsEnabled);

        if (product == null)
            return null;

        // Calculate total stock and map variant stocks
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
        // Load approved reviews with user info, ordered by most recent
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
     * Maps a product variant to DTO with calculated price (including sale discount)
     */
    private ProductVariantDto MapToVariantDto(ProductVariant variant)
    {
        // Use variant-specific price override, or fallback to product base price
        var price = variant.PriceOverride ?? variant.Product.Price;
        
        // Apply sale discount if active promotion exists
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
            variant.Color != null ? new ColorDto(variant.Color.Id, variant.Color.Name, variant.Color.Hexa) : null,
            variant.StockCount,
            price,
            variant.StockCount > 0,
            variant.Media.OrderBy(m => m.DisplayOrder).Select(m => new ProductVariantMediaDto(
                m.Id,
                m.Url,
                m.Type,
                m.DisplayOrder,
                m.IsPrimary
            ))
        );
    }

    /**
     * Maps a product review to DTO with user full name
     */
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

    /**
     * Maps a sale/promotion to DTO
     */
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

    /**
     * Retrieves paginated and filtered products for a specific brand
     */
    public async Task<ProductsListResponse> GetProductsByBrandAsync(GetProductsQuery query)
    {
        // Verify brand exists and is approved
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == query.BrandId && b.Status == BrandStatus.Approved);

        if (brand == null)
            throw new InvalidOperationException("Brand not found or not approved");

        // Build base query for online products with reviews and variants
        var productsQuery = _context.Products
            .Where(p => p.BrandId == query.BrandId && p.Status == ProductStatus.Online) 
            .Include(p => p.Reviews)
            .Include(p => p.Variants) 
                .ThenInclude(v => v.Media)
            .Include(p => p.Category) 
            .AsQueryable();

        // Apply category filter if specified
        if (!string.IsNullOrEmpty(query.Category))
        {
            productsQuery = productsQuery.Where(p => p.Category.Name == query.Category);
        }

        // Apply price range filters
        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);
        }

        // Apply search term filter on name and description
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            productsQuery = productsQuery.Where(p => 
                p.Name.ToLower().Contains(searchTerm) || 
                p.Description.ToLower().Contains(searchTerm)
            );
        }

        var products = await productsQuery.ToListAsync();

        // Calculate average rating for each product
        var enrichedProducts = products
            .Select(p => new
            {
                Product = p,
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => (double)r.Rating) : 0.0
            })
            .ToList();

        // Apply sorting based on query parameter
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

        // Apply pagination and map to summary DTOs
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
     * Maps a product to summary DTO with primary image and rating
     */
    private ProductSummaryDto MapToProductSummary(Product product, double averageRating)
    {
        // Find primary image from variants, or fallback to first available image
        var primaryImage = product.Variants
            .SelectMany(v => v.Media)
            .Where(m => m.IsPrimary)
            .OrderBy(m => m.DisplayOrder)
            .FirstOrDefault()?.Url;

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
            primaryImage, 
            product.Price,
            product.Description,
            Math.Round(averageRating, 1),
            product.Reviews.Count
        );
    }
}
