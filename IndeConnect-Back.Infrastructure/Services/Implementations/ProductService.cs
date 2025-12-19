using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.DTOs.Products;
using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.catalog.product;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    public ProductService(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
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
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var query = _context.ProductReviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt);

        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return reviews.Select(r => new ProductReviewDto(
            r.Id,
            r.UserId,
            r.User.FirstName + " " + r.User.LastName,
            r.Rating,
            r.Comment,
            r.CreatedAt,
            r.Status
        ));
    }



    /**
     * Retrieves paginated and filtered products for a specific brand
     * IMPORTANT: Returns individual products (each color = separate entry)
     */
    public async Task<ProductsListResponse> GetProductsByBrandAsync(GetProductsQuery query, long? userId)
    {
        UserDetailDto? user = null;
        
        if (userId.HasValue)
        {
            user = await _userService.GetUserByIdAsync(userId);
        }
        
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == query.BrandId);

        if (brand == null)
            throw new InvalidOperationException("Brand not found");

        // 🆕 MODIFICATION : Vérifier si l'utilisateur peut voir les produits non-Online
        bool canSeeAllProducts = user != null && 
            (user.role == Role.SuperVendor && brand.SuperVendorUserId == userId) ||
            (user.role == Role.Administrator) ||
            (user.role == Role.Moderator);

        // Si la marque n'est pas approuvée, seul le SuperVendor peut voir ses produits
        if (brand.Status != BrandStatus.Approved && !canSeeAllProducts)
            throw new InvalidOperationException("Brand not found or not approved");

        // 🆕 FILTRE CONDITIONNEL
        var productsQuery = _context.Products
            .Where(p => p.BrandId == query.BrandId)
            .Include(p => p.Reviews)
            .Include(p => p.Variants) 
            .Include(p => p.Media)
            .Include(p => p.Category) 
            .Include(p => p.PrimaryColor)
            .AsQueryable();

        // 🆕 Appliquer le filtre de statut selon le rôle
        if (!canSeeAllProducts)
        {
            // Les clients/visiteurs ne voient que les produits Online
            productsQuery = productsQuery.Where(p => p.Status == ProductStatus.Online);
        }
        // Sinon (SuperVendor/Admin/Moderator), on ne filtre PAS le statut

        // Apply filters (reste inchangé)
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
    public async Task<CreateProductResponse> CreateProductAsync(CreateProductQuery query, long? currentUserId)
    {
        if (string.IsNullOrWhiteSpace(query.Name)) 
            throw new ArgumentException("Product name is required.", nameof(query.Name));
        if (query.Price <= 0) 
            throw new ArgumentOutOfRangeException(nameof(query.Price), "Price must be positive.");

        // VÉRIFICATION : L'utilisateur doit être SuperVendor de la marque
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == query.BrandId);
        
        if (brand == null)
            throw new InvalidOperationException($"Brand with id {query.BrandId} not found.");
        
        if (brand.SuperVendorUserId != currentUserId)
            throw new UnauthorizedAccessException("You are not the SuperVendor of this brand.");

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == query.CategoryId);
        if (!categoryExists)
            throw new InvalidOperationException($"Category with id {query.CategoryId} not found.");

        // VÉRIFICATION : Le ProductGroup doit exister et appartenir à la même marque
        var productGroup = await _context.ProductGroups
            .FirstOrDefaultAsync(pg => pg.Id == query.ProductGroupId);

        if (productGroup == null)
            throw new InvalidOperationException($"Product group with id {query.ProductGroupId} not found.");

        if (productGroup.BrandId != query.BrandId)
            throw new InvalidOperationException($"Product group {query.ProductGroupId} does not belong to brand {query.BrandId}.");

        var product = new Product(
            name: query.Name,
            description: query.Description,
            price: query.Price,
            brandId: query.BrandId,
            categoryId: query.CategoryId,
            productGroupId: query.ProductGroupId,
            primaryColorId: query.PrimaryColorId
        );

        _context.Products.Add(product);
        
        // IMPORTANT : Sauvegarder d'abord pour obtenir l'Id du produit
        await _context.SaveChangesAsync();

        // Maintenant que product.Id est généré, on peut ajouter les entités liées

        // Ajouter les médias
        if (query.Media != null && query.Media.Any())
        {
            foreach (var mediaDto in query.Media)
            {
                var media = new ProductMedia(
                    productId: product.Id,
                    url: mediaDto.Url,
                    type: mediaDto.Type,
                    displayOrder: mediaDto.DisplayOrder,
                    isPrimary: mediaDto.IsPrimary
                );
                _context.ProductMedia.Add(media); // Utilise DbSet directement
            }
        }

        // Ajouter les variantes de taille
        if (query.SizeVariants != null && query.SizeVariants.Any())
        {
            foreach (var variantDto in query.SizeVariants)
            {
                var variant = new ProductVariant(
                    productId: product.Id,
                    sizeId: variantDto.Size?.Id,
                    sku: variantDto.SKU,
                    stockCount: variantDto.StockCount, // OU stockCount selon ton constructeur
                    priceOverride: variantDto.Price != product.Price ? variantDto.Price : null
                );
                _context.ProductVariants.Add(variant);
            }
        }

        // Ajouter les détails
        if (query.Details != null && query.Details.Any())
        {
            foreach (var detailDto in query.Details)
            {
                var detail = new ProductDetail(
                    productId: product.Id,
                    value: detailDto.Value,
                    displayOrder: detailDto.DisplayOrder
                );
                _context.ProductDetails.Add(detail);
            }
        }

        // Ajouter les keywords
        if (query.Keywords != null && query.Keywords.Any())
        {
            foreach (var keywordName in query.Keywords)
            {
                var keyword = await _context.Keywords
                    .FirstOrDefaultAsync(k => k.Name == keywordName);
                
                if (keyword == null)
                {
                    keyword = new Keyword(keywordName);
                    _context.Keywords.Add(keyword);
                    await _context.SaveChangesAsync(); // Sauvegarder pour obtenir keyword.Id
                }
                
                // Utilise le constructeur avec 2 paramètres
                var productKeyword = new ProductKeyword(product.Id, keyword.Id);
                _context.ProductKeywords.Add(productKeyword);
            }
        }

        await _context.SaveChangesAsync();

        var categoryName = await _context.Categories
            .Where(c => c.Id == query.CategoryId)
            .Select(c => c.Name)
            .FirstAsync();

        return new CreateProductResponse(
            Id: product.Id,
            Name: product.Name,
            BrandName: brand.Name,
            CategoryName: categoryName,
            Price: product.Price,
            Status: product.Status,
            CreatedAt: product.CreatedAt
        );
    }
public async Task<UpdateProductResponse> UpdateProductAsync(long productId, UpdateProductQuery query, long? currentUserId)
{
    if (string.IsNullOrWhiteSpace(query.Name)) 
        throw new ArgumentException("Product name is required.", nameof(query.Name));
    if (query.Price <= 0) 
        throw new ArgumentOutOfRangeException(nameof(query.Price), "Price must be positive.");

    var product = await _context.Products
        .Include(p => p.Brand)
        .FirstOrDefaultAsync(p => p.Id == productId);

    if (product == null) 
        throw new KeyNotFoundException($"Product with id {productId} not found.");

    // VÉRIFICATION : L'utilisateur doit être SuperVendor de la marque
    if (product.Brand.SuperVendorUserId != currentUserId)
        throw new UnauthorizedAccessException("You are not the SuperVendor of this product's brand.");

    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == query.CategoryId);
    if (!categoryExists) 
        throw new InvalidOperationException($"Category with id {query.CategoryId} not found.");

    product.UpdateInfo(
        name: query.Name,
        description: query.Description,
        price: query.Price,
        categoryId: query.CategoryId,
        primaryColorId: query.PrimaryColorId,
        status: query.Status
    );

    await _context.SaveChangesAsync();

    var categoryName = await _context.Categories
        .Where(c => c.Id == product.CategoryId)
        .Select(c => c.Name)
        .FirstAsync();

    return new UpdateProductResponse(
        Id: product.Id,
        Name: product.Name,
        BrandName: product.Brand.Name,
        CategoryName: categoryName,
        Price: product.Price,
        Status: product.Status,
        UpdatedAt: product.UpdatedAt!.Value
    );
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
                : null,
            product.Status // 🆕 AJOUTE LE STATUT
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
    
    private async Task<bool> HasUserPurchasedProductAsync(long userId, long productId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .AnyAsync(o =>
                o.UserId == userId &&
                (o.Status == OrderStatus.Paid || o.Status == OrderStatus.Delivered) &&
                o.Items.Any(i => i.ProductId == productId));
    }
    
    public async Task<ProductReviewDto> AddProductReviewAsync(long productId, long userId, int rating, string? comment)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsEnabled);
        if (product == null)
            throw new KeyNotFoundException("Product not found");

        var hasPurchased = await HasUserPurchasedProductAsync(userId, productId);
        if (!hasPurchased)
            throw new InvalidOperationException("User has not purchased this product");

        var alreadyReviewed = await _context.ProductReviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);
        if (alreadyReviewed)
            throw new InvalidOperationException("User has already reviewed this product");

        var review = new ProductReview(productId, userId, rating, comment);

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync();

        review = await _context.ProductReviews
            .Include(r => r.User)
            .FirstAsync(r => r.Id == review.Id);

        return new ProductReviewDto(
            review.Id,
            review.UserId,
            review.User.FirstName + " " + review.User.LastName,
            review.Rating,
            review.Comment,
            review.CreatedAt,
            review.Status
        );
    }
    
    public async Task<IEnumerable<ProductReviewDto>> GetAllProductReviewsAsync(long productId)
    {
        var reviews = await _context.ProductReviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(r => new ProductReviewDto(
            r.Id,
            r.UserId,
            r.User.FirstName + " " + r.User.LastName,
            r.Rating,
            r.Comment,
            r.CreatedAt,
            r.Status
        ));
    }

    private async Task<bool> IsSellerOfProductAsync(long sellerUserId, long productId)
    {
        var product = await _context.Products
            .Include(p => p.Brand)
            .ThenInclude(b => b.Sellers)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            return false;

        var brand = product.Brand;
        if (brand == null)
            return false;

        return (brand.SuperVendorUserId == sellerUserId)
               || brand.Sellers.Any(s => s.SellerId == sellerUserId && s.IsActive);
    }

    public async Task ApproveProductReviewAsync(long reviewId, long sellerUserId)
    {
        var review = await _context.ProductReviews
            .Include(r => r.Product)
            .ThenInclude(p => p.Brand)
            .ThenInclude(b => b.Sellers)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new KeyNotFoundException("Review not found");

        var isSeller = await IsSellerOfProductAsync(sellerUserId, review.ProductId);
        if (!isSeller)
            throw new UnauthorizedAccessException("User is not seller of this product");

        review.Approve();
        await _context.SaveChangesAsync();
    }

    public async Task RejectProductReviewAsync(long reviewId, long sellerUserId)
    {
        var review = await _context.ProductReviews
            .Include(r => r.Product)
            .ThenInclude(p => p.Brand)
            .ThenInclude(b => b.Sellers)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new KeyNotFoundException("Review not found");

        var isSeller = await IsSellerOfProductAsync(sellerUserId, review.ProductId);
        if (!isSeller)
            throw new UnauthorizedAccessException("User is not seller of this product");

        review.Reject();
        await _context.SaveChangesAsync();
    }

}
