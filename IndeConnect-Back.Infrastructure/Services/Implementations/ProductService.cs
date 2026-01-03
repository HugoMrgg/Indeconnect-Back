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
    private readonly IEmailService _emailService;

    public ProductService(AppDbContext context, IUserService userService, IEmailService emailService)
    {
        _context = context;
        _userService = userService;
        _emailService = emailService;
    }

    /// <summary>
    /// Vérifie si l'utilisateur a accès à la marque (SuperVendor ou Vendor)
    /// </summary>
    private async Task<bool> HasBrandAccessAsync(long? userId, long brandId)
    {
        if (userId == null) return false;

        var brand = await _context.Brands
            .Include(b => b.Sellers)
            .FirstOrDefaultAsync(b => b.Id == brandId);

        if (brand == null) return false;

        // Vérifier si l'utilisateur est le SuperVendor de la marque
        if (brand.SuperVendorUserId == userId) return true;

        // Vérifier si l'utilisateur est un Vendor actif de la marque
        return brand.Sellers.Any(bs => bs.SellerId == userId && bs.IsActive);
    }

    /// <summary>
    /// Envoie un email à tous les abonnés d'une marque
    /// </summary>
    private async Task NotifyBrandSubscribersAsync(long brandId, string subject, string emailContent)
    {
        var subscribers = await _context.BrandSubscriptions
            .Include(bs => bs.User)
            .Where(bs => bs.BrandId == brandId)
            .Select(bs => new { bs.User.Email, bs.User.FirstName })
            .ToListAsync();

        foreach (var subscriber in subscribers)
        {
            var personalizedContent = emailContent.Replace("{FirstName}", subscriber.FirstName);
            await _emailService.SendEmailAsync(subscriber.Email, subject, personalizedContent);
        }
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
            .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Enabled))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsEnabled);

        if (product == null)
            return null;

        // Utiliser les méthodes métier du Domain
        var currentPrice = product.CalculateCurrentPrice();
        var basePrice = product.Price;
        var salePrice = currentPrice != basePrice ? currentPrice : (decimal?)null;

        var totalStock = product.GetTotalStock();
        var isAvailable = product.IsAvailableForPurchase();

        var avgRating = product.GetAverageRating();
        var reviewsCount = product.GetApprovedReviewsCount();

        // Récupérer les autres couleurs disponibles - utiliser les méthodes du Domain
        var colorVariants = product.ProductGroup?.GetOnlineProducts()
            .Where(p => p.Id != productId)
            .Select(p => new ProductColorVariantDto(
                p.Id,
                p.PrimaryColor?.Id,
                p.PrimaryColor?.Name,
                p.PrimaryColor?.Hexa,
                p.GetPrimaryImageUrl(),
                p.IsAvailableForPurchase()
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
            product.CategoryId, // ← AJOUTER CETTE LIGNE (manquait dans ton code)
            product.PrimaryColor != null 
                ? new ColorDto(product.PrimaryColor.Id, product.PrimaryColor.Name, product.PrimaryColor.Hexa) 
                : null, 
            colorVariants,
            product.Media.OrderBy(m => m.DisplayOrder).Select(m => new ProductMediaDto(
                m.Url,
                m.Type,
                m.DisplayOrder,
                m.IsPrimary
            )),
            product.Variants.Select(MapToVariantDto),
            product.Details.OrderBy(d => d.DisplayOrder).Select(d => new ProductDetailItemDto(d.Value, d.DisplayOrder)),
            product.Keywords.Select(pk => pk.Keyword.Name),
            product.Reviews.Where(r => r.Status == ReviewStatus.Enabled).Select(MapToReviewDto),
            Math.Round(avgRating, 1),
            reviewsCount,
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

        // Utiliser les méthodes du Domain
        return product.ProductGroup.GetOnlineProducts()
            .Select(p => new ProductColorVariantDto(
                p.Id,
                p.PrimaryColor?.Id,
                p.PrimaryColor?.Name,
                p.PrimaryColor?.Hexa,
                p.GetPrimaryImageUrl(),
                p.IsAvailableForPurchase()
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
            // Utiliser les méthodes du Domain
            group.GetOnlineProducts()
                .Select(p => new ProductColorVariantDto(
                    p.Id,
                    p.PrimaryColor?.Id,
                    p.PrimaryColor?.Name,
                    p.PrimaryColor?.Hexa,
                    p.GetPrimaryImageUrl(),
                    p.IsAvailableForPurchase()
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

        // Utiliser la méthode métier du Domain
        var totalStock = product.GetTotalStock();
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
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Enabled)
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
        user = await _userService.GetUserByIdAsync(userId);

    var brand = await _context.Brands
        .FirstOrDefaultAsync(b => b.Id == query.BrandId);

    if (brand == null)
        throw new InvalidOperationException("Brand not found.");

    // ✅ Vérifier si l'utilisateur peut voir les produits (même si marque non approuvée)
    bool canSeeAllProducts = false;

    if (user != null)
    {
        if (user.role == Role.SuperVendor && brand.SuperVendorUserId == userId)
        {
            canSeeAllProducts = true;
        }
        else if (user.role == Role.Vendor)
        {
            // ✅ Vérifier via BrandSellers (comme dans BrandService)
            var isVendorOfBrand = await _context.BrandSellers
                .AnyAsync(bs => bs.SellerId == userId && bs.BrandId == brand.Id && bs.IsActive);
            
            canSeeAllProducts = isVendorOfBrand;
        }
        else if (user.role == Role.Administrator || user.role == Role.Moderator)
        {
            canSeeAllProducts = true;
        }
    }

    // ✅ Si la marque n'est pas approuvée ET l'utilisateur n'a pas les droits → erreur
    if (brand.Status != BrandStatus.Approved && !canSeeAllProducts)
        throw new InvalidOperationException("Brand not found or not approved.");

    // Récupérer les produits
    var productsQuery = _context.Products
        .Where(p => p.BrandId == query.BrandId)
        .Include(p => p.Reviews)
        .Include(p => p.Variants)
        .Include(p => p.Media)
        .Include(p => p.Category)
        .Include(p => p.PrimaryColor)
        .Include(p => p.Sale)
        .AsQueryable();

    // ✅ Appliquer le filtre de statut selon le rôle
    if (!canSeeAllProducts)
    {
        // Client ou non-authentifié : uniquement produits Online
        productsQuery = productsQuery.Where(p => p.Status == ProductStatus.Online);
    }
    // Sinon (SuperVendor/Vendor/Admin) : voir tous les produits (Draft, Online, Archived)

    // Apply filters
    if (!string.IsNullOrEmpty(query.Category))
        productsQuery = productsQuery.Where(p => p.Category.Name == query.Category);

    if (query.MinPrice.HasValue)
        productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);

    if (query.MaxPrice.HasValue)
        productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);

    if (!string.IsNullOrEmpty(query.SearchTerm))
    {
        var searchTerm = query.SearchTerm.ToLower();
        productsQuery = productsQuery.Where(p =>
            p.Name.ToLower().Contains(searchTerm) ||
            p.Description.ToLower().Contains(searchTerm)
        );
    }

    var products = await productsQuery.ToListAsync();

    // Enrichir avec les ratings
    var enrichedProducts = products
        .Select(p => new
        {
            Product = p,
            AverageRating = p.Reviews != null && p.Reviews.Any()
                ? p.Reviews.Average(r => (double)r.Rating)
                : 0.0
        })
        .ToList();

    // Tri
    var sortedProducts = query.SortBy switch
    {
        ProductSortType.PriceAsc => enrichedProducts.OrderBy(x => x.Product.Price).ToList(),
        ProductSortType.PriceDesc => enrichedProducts.OrderByDescending(x => x.Product.Price).ToList(),
        ProductSortType.Rating => enrichedProducts.OrderByDescending(x => x.AverageRating).ToList(),
        ProductSortType.Popular => enrichedProducts.OrderByDescending(x => x.Product.Reviews?.Count ?? 0).ToList(),
        ProductSortType.Newest => enrichedProducts.OrderByDescending(x => x.Product.CreatedAt).ToList(),
        _ => enrichedProducts.OrderByDescending(x => x.Product.CreatedAt).ToList()
    };

    var totalCount = sortedProducts.Count;

    var paginatedProducts = sortedProducts
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .Select(x => MapToProductSummary(x.Product, x.AverageRating))
        .ToList();

    return new ProductsListResponse(paginatedProducts, totalCount, query.Page, query.PageSize);
}
    public async Task<CreateProductResponse> CreateProductAsync(CreateProductQuery query, long? currentUserId)
    {
        if (string.IsNullOrWhiteSpace(query.Name)) 
            throw new ArgumentException("Product name is required.", nameof(query.Name));
        if (query.Price <= 0) 
            throw new ArgumentOutOfRangeException(nameof(query.Price), "Price must be positive.");

        // VÉRIFICATION : L'utilisateur doit être SuperVendor ou Vendor de la marque
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == query.BrandId);

        if (brand == null)
            throw new InvalidOperationException($"Brand with id {query.BrandId} not found.");

        if (!await HasBrandAccessAsync(currentUserId, query.BrandId))
            throw new UnauthorizedAccessException("You do not have access to this brand.");

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == query.CategoryId);
        if (!categoryExists)
            throw new InvalidOperationException($"Category with id {query.CategoryId} not found.");

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

        // Notifier les abonnés de la marque du nouveau produit
        var newProductEmailContent = BuildNewProductEmailHtml(brand.Name, product.Name, product.Description);
        await NotifyBrandSubscribersAsync(
            brand.Id,
            $"Nouveau produit chez {brand.Name} !",
            newProductEmailContent
        );

        return new CreateProductResponse(
            Id: product.Id,
            Name: product.Name,
            BrandName: brand.Name,
            CategoryName: categoryName,
            Price: product.Price,
            Status: product.Status,
            CreatedAt: product.CreatedAt
        );
    }public async Task<UpdateProductResponse> UpdateProductAsync(long productId, UpdateProductQuery query, long? currentUserId)
{
    if (string.IsNullOrWhiteSpace(query.Name)) 
        throw new ArgumentException("Product name is required.", nameof(query.Name));
    if (query.Price <= 0) 
        throw new ArgumentOutOfRangeException(nameof(query.Price), "Price must be positive.");

    var product = await _context.Products
        .Include(p => p.Brand)
        .Include(p => p.Sale)
        .Include(p => p.Media)
        .Include(p => p.Variants)
        .FirstOrDefaultAsync(p => p.Id == productId);

    if (product == null)
        throw new KeyNotFoundException($"Product with id {productId} not found.");

    // VÉRIFICATION : L'utilisateur doit être SuperVendor ou Vendor de la marque
    if (!await HasBrandAccessAsync(currentUserId, product.BrandId))
        throw new UnauthorizedAccessException("You do not have access to this product's brand.");

    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == query.CategoryId);
    if (!categoryExists) 
        throw new InvalidOperationException($"Category with id {query.CategoryId} not found.");

    // 1. Mettre à jour les infos de base
    product.UpdateInfo(
        name: query.Name,
        description: query.Description,
        price: query.Price,
        categoryId: query.CategoryId,
        primaryColorId: query.PrimaryColorId,
        status: query.Status
    );

    // 2. Gérer la promotion (Sale)
    bool shouldNotifyPromotion = false;
    decimal? discountPercentage = null;

    if (query.Sale != null)
    {
        // ← NOUVEAU : Convertir les dates en UTC pour PostgreSQL
        var startDateUtc = query.Sale.StartDate.ToUniversalTime();
        var endDateUtc = query.Sale.EndDate.ToUniversalTime();

        if (product.Sale == null)
        {
            // Créer une nouvelle promotion
            var newSale = new Sale(
                name: $"Promo {product.Name}",
                description: query.Sale.Description ?? "",
                discountPercentage: query.Sale.DiscountPercentage,
                startDate: startDateUtc,  // ← MODIFIÉ
                endDate: endDateUtc        // ← MODIFIÉ
            );
            _context.Sales.Add(newSale);
            await _context.SaveChangesAsync();

            product.SetSale(newSale.Id);
            shouldNotifyPromotion = true;
            discountPercentage = query.Sale.DiscountPercentage;
        }
        else
        {
            // Mettre à jour la promotion existante
            _context.Sales.Remove(product.Sale);

            var updatedSale = new Sale(
                name: $"Promo {product.Name}",
                description: query.Sale.Description ?? "",
                discountPercentage: query.Sale.DiscountPercentage,
                startDate: startDateUtc,  // ← MODIFIÉ
                endDate: endDateUtc        // ← MODIFIÉ
            );
            _context.Sales.Add(updatedSale);
            await _context.SaveChangesAsync();

            product.SetSale(updatedSale.Id);
            shouldNotifyPromotion = true;
            discountPercentage = query.Sale.DiscountPercentage;
        }
    }
    else
    {
        // Supprimer la promotion si elle existe
        if (product.Sale != null)
        {
            _context.Sales.Remove(product.Sale);
            product.RemoveSale();
        }
    }

    // 3. Gérer les médias
    if (query.Media != null && query.Media.Any())
    {
        // Supprimer tous les anciens médias
        var existingMedia = await _context.ProductMedia
            .Where(m => m.ProductId == productId)
            .ToListAsync();
        
        _context.ProductMedia.RemoveRange(existingMedia);

        // Ajouter les nouveaux médias
        foreach (var mediaDto in query.Media)
        {
            var media = new ProductMedia(
                productId: product.Id,
                url: mediaDto.Url,
                type: mediaDto.Type,
                displayOrder: mediaDto.DisplayOrder,
                isPrimary: mediaDto.IsPrimary
            );
            _context.ProductMedia.Add(media);
        }
    }

    // 4. Gérer les variantes (tailles)
    if (query.Variants != null && query.Variants.Any())
    {
        // Récupérer les variantes existantes
        var existingVariants = await _context.ProductVariants
            .Where(v => v.ProductId == productId)
            .ToListAsync();

        // Identifier les variantes à supprimer (celles qui ne sont plus dans la requête)
        var variantsToRemove = existingVariants
            .Where(ev => !query.Variants.Any(qv => qv.Id == ev.Id))
            .ToList();
        
        _context.ProductVariants.RemoveRange(variantsToRemove);

        foreach (var variantDto in query.Variants)
        {
            if (variantDto.Id.HasValue)
            {
                // Mise à jour d'une variante existante
                var existingVariant = existingVariants.FirstOrDefault(v => v.Id == variantDto.Id.Value);
                if (existingVariant != null)
                {
                    existingVariant.UpdateStock(variantDto.StockCount);
                }
            }
            else
            {
                // Création d'une nouvelle variante
                var newVariant = new ProductVariant(
                    productId: product.Id,
                    sizeId: variantDto.SizeId,
                    sku: $"{product.Id}-{variantDto.SizeId}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                    stockCount: variantDto.StockCount,
                    priceOverride: null
                );
                _context.ProductVariants.Add(newVariant);
            }
        }
    }

    await _context.SaveChangesAsync();

    var categoryName = await _context.Categories
        .Where(c => c.Id == product.CategoryId)
        .Select(c => c.Name)
        .FirstAsync();

    // Notifier les abonnés si une promotion a été ajoutée
    if (shouldNotifyPromotion && discountPercentage.HasValue)
    {
        var promoEmailContent = BuildPromotionEmailHtml(
            product.Brand.Name,
            product.Name,
            discountPercentage.Value,
            product.Price
        );
        await NotifyBrandSubscribersAsync(
            product.BrandId,
            $"🎉 Promotion sur {product.Name} chez {product.Brand.Name} !",
            promoEmailContent
        );
    }

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
        // Utiliser la méthode métier du Domain pour calculer le prix
        var price = variant.PriceOverride ?? variant.Product.CalculateCurrentPrice();

        return new ProductVariantDto(
            Id: variant.Id,
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
        var primaryImage = product.Media?
            .Where(m => m.IsPrimary)
            .OrderBy(m => m.DisplayOrder)
            .FirstOrDefault()?.Url;

        if (primaryImage == null)
        {
            primaryImage = product.Media?
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
            product.Reviews?.Count ?? 0,
            product.PrimaryColor != null 
                ? new ColorDto(product.PrimaryColor.Id, product.PrimaryColor.Name, product.PrimaryColor.Hexa)
                : null,
            product.Status,
            product.Sale != null ? MapToSaleDto(product.Sale) : null 
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
    
    public async Task<bool> CanUserReviewProductAsync(long userId, long productId)
    {
        return await _context.Orders
            .AnyAsync(o => 
                o.UserId == userId &&
                o.Status.ToString() == "Delivered" &&
                o.Items.Any(i => i.ProductId == productId)
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

    /// <summary>
    /// Construit le template HTML pour l'email de nouveau produit
    /// </summary>
    private static string BuildNewProductEmailHtml(string brandName, string productName, string productDescription)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #000; color: #fff; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .product-name {{ font-size: 24px; font-weight: bold; color: #000; margin: 15px 0; }}
        .button {{
            display: inline-block;
            background-color: #000;
            color: #fff;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 5px;
            margin-top: 20px;
        }}
        .footer {{ text-align: center; padding-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{brandName}</h1>
        </div>
        <div class=""content"">
            <p>Bonjour {{FirstName}},</p>
            <p>Nous sommes ravis de vous annoncer l'arrivée d'un nouveau produit chez <strong>{brandName}</strong> !</p>
            <div class=""product-name"">{productName}</div>
            <p>{productDescription}</p>
            <a href=""http://localhost:5173/products"" class=""button"">Découvrir le produit</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2025 IndeConnect. Tous droits réservés.</p>
            <p>Vous recevez cet email car vous êtes abonné à {brandName}.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Construit le template HTML pour l'email de promotion
    /// </summary>
    private static string BuildPromotionEmailHtml(string brandName, string productName, decimal discountPercentage, decimal originalPrice)
    {
        var discountedPrice = originalPrice * (1 - discountPercentage / 100);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ff0000; color: #fff; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .product-name {{ font-size: 24px; font-weight: bold; color: #000; margin: 15px 0; }}
        .discount-badge {{
            background-color: #ff0000;
            color: #fff;
            padding: 10px 20px;
            border-radius: 25px;
            font-size: 20px;
            font-weight: bold;
            display: inline-block;
            margin: 15px 0;
        }}
        .price-container {{ margin: 20px 0; }}
        .original-price {{
            text-decoration: line-through;
            color: #999;
            font-size: 18px;
        }}
        .sale-price {{
            color: #ff0000;
            font-size: 28px;
            font-weight: bold;
            margin-left: 10px;
        }}
        .button {{
            display: inline-block;
            background-color: #ff0000;
            color: #fff;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 5px;
            margin-top: 20px;
        }}
        .footer {{ text-align: center; padding-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 PROMOTION CHEZ {brandName} ! 🎉</h1>
        </div>
        <div class=""content"">
            <p>Bonjour {{FirstName}},</p>
            <p>Ne manquez pas cette offre exceptionnelle sur un produit de <strong>{brandName}</strong> !</p>
            <div class=""product-name"">{productName}</div>
            <div class=""discount-badge"">-{discountPercentage:F0}%</div>
            <div class=""price-container"">
                <span class=""original-price"">{originalPrice:F2}€</span>
                <span class=""sale-price"">{discountedPrice:F2}€</span>
            </div>
            <p style=""color: #ff0000; font-weight: bold;"">Profitez-en vite, offre limitée !</p>
            <a href=""http://localhost:5173/products"" class=""button"">Voir la promotion</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2025 IndeConnect. Tous droits réservés.</p>
            <p>Vous recevez cet email car vous êtes abonné à {brandName}.</p>
        </div>
    </div>
</body>
</html>";
    }

}
