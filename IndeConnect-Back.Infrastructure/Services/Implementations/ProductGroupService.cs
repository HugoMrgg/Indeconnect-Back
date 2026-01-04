using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.DTOs.Products;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.product;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class ProductGroupService : IProductGroupService
{
    private readonly AppDbContext _context;

    public ProductGroupService(AppDbContext context)
    {
        _context = context;
    }

public async Task<ProductGroupDto> CreateProductGroupAsync(CreateProductGroupRequest request, long? currentUserId)
{
    if (string.IsNullOrWhiteSpace(request.Name))
        throw new ArgumentException("Product group name is required.", nameof(request.Name));
    if (string.IsNullOrWhiteSpace(request.BaseDescription))
        throw new ArgumentException("Product group description is required.", nameof(request.BaseDescription));

    // Récupérer l'utilisateur
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == currentUserId);

    if (user == null)
        throw new UnauthorizedAccessException("User not found.");

    long? brandId = null;

    // SuperVendor : a un BrandId direct
    if (user.BrandId.HasValue)
    {
        brandId = user.BrandId.Value;
    }
    // Vendor : chercher via BrandSellers
    else
    {
        var activeBrandSeller = await _context.BrandSellers
            .Where(bs => bs.SellerId == currentUserId && bs.IsActive)
            .FirstOrDefaultAsync();

        if (activeBrandSeller != null)
        {
            brandId = activeBrandSeller.BrandId;
        }
    }

    if (!brandId.HasValue)
        throw new UnauthorizedAccessException("You must be associated with a brand to create product groups.");

    // Vérifier que la catégorie existe
    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
    if (!categoryExists)
        throw new InvalidOperationException($"Category with id {request.CategoryId} not found.");

    // Créer le ProductGroup
    var productGroup = new ProductGroup(
        name: request.Name,
        baseDescription: request.BaseDescription,
        brandId: brandId.Value,
        categoryId: request.CategoryId
    );

    _context.ProductGroups.Add(productGroup);
    await _context.SaveChangesAsync();

    // Recharger avec les relations pour le DTO
    var createdGroup = await _context.ProductGroups
        .Include(pg => pg.Brand)
        .Include(pg => pg.Category)
        .Include(pg => pg.Products)
            .ThenInclude(p => p.PrimaryColor)
        .Include(pg => pg.Products)
            .ThenInclude(p => p.Media)
        .Include(pg => pg.Products)
            .ThenInclude(p => p.Variants)
        .FirstOrDefaultAsync(pg => pg.Id == productGroup.Id);

    return MapToDto(createdGroup!);
}
    public async Task<ProductGroupDto?> GetProductGroupByIdAsync(long productGroupId)
    {
        var productGroup = await _context.ProductGroups
            .Include(pg => pg.Brand)
            .Include(pg => pg.Category)
            .Include(pg => pg.Products.Where(p => p.IsEnabled && p.Status == ProductStatus.Online))
                .ThenInclude(p => p.PrimaryColor)
            .Include(pg => pg.Products)
                .ThenInclude(p => p.Media)
            .Include(pg => pg.Products)
                .ThenInclude(p => p.Variants)
            .FirstOrDefaultAsync(pg => pg.Id == productGroupId);

        return productGroup == null ? null : MapToDto(productGroup);
    }

    public async Task<IEnumerable<ProductGroupSummaryDto>> GetProductGroupsByBrandAsync(long brandId)
    {
        var productGroups = await _context.ProductGroups
            .Include(pg => pg.Category)
            .Include(pg => pg.Products)
            .Where(pg => pg.BrandId == brandId)
            .OrderByDescending(pg => pg.Id)
            .ToListAsync();

        return productGroups.Select(pg => new ProductGroupSummaryDto(
            pg.Id,
            pg.Name,
            pg.BaseDescription,
            pg.CategoryId,
            pg.Category.Name,
            pg.Products.Count
        ));
    }

    public async Task<ProductGroupDto> UpdateProductGroupAsync(long productGroupId, UpdateProductGroupRequest request, long? currentUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Product group name is required.", nameof(request.Name));
        if (string.IsNullOrWhiteSpace(request.BaseDescription))
            throw new ArgumentException("Product group description is required.", nameof(request.BaseDescription));

        var productGroup = await _context.ProductGroups
            .Include(pg => pg.Brand)
            .FirstOrDefaultAsync(pg => pg.Id == productGroupId);

        if (productGroup == null)
            throw new KeyNotFoundException($"Product group with id {productGroupId} not found.");

        // ✅ Vérifier accès (SuperVendor OU Vendor)
        var hasAccess = await HasBrandAccessAsync(currentUserId, productGroup.BrandId);
        if (!hasAccess)
            throw new UnauthorizedAccessException("You do not have access to this brand.");

        // Vérifier que la catégorie existe
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new InvalidOperationException($"Category with id {request.CategoryId} not found.");

        // Mettre à jour
        productGroup.UpdateInfo(request.Name, request.BaseDescription, request.CategoryId);

        await _context.SaveChangesAsync();

        // Recharger avec toutes les relations
        var updatedGroup = await _context.ProductGroups
            .Include(pg => pg.Brand)
            .Include(pg => pg.Category)
            .Include(pg => pg.Products.Where(p => p.IsEnabled && p.Status == ProductStatus.Online))
            .ThenInclude(p => p.PrimaryColor)
            .Include(pg => pg.Products)
            .ThenInclude(p => p.Media)
            .Include(pg => pg.Products)
            .ThenInclude(p => p.Variants)
            .FirstOrDefaultAsync(pg => pg.Id == productGroupId);

        return MapToDto(updatedGroup!);
    }
    private async Task<bool> HasBrandAccessAsync(long? userId, long brandId)
    {
        if (userId == null)
            return false;

        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == brandId);
        if (brand == null)
            return false;

        // SuperVendor
        if (brand.SuperVendorUserId == userId)
            return true;

        // Vendor (via BrandSellers)
        var isVendor = await _context.BrandSellers
            .AnyAsync(bs => bs.SellerId == userId && bs.BrandId == brandId && bs.IsActive);

        return isVendor;
    }

    private ProductGroupDto MapToDto(ProductGroup productGroup)
    {
        return new ProductGroupDto(
            productGroup.Id,
            productGroup.Name,
            productGroup.BaseDescription,
            new BrandSummaryDto(
                productGroup.Brand.Id,
                productGroup.Brand.Name,
                productGroup.Brand.LogoUrl,
                productGroup.Brand.BannerUrl,
                productGroup.Brand.Description,
                0, 0, Enumerable.Empty<string>(), null, null, 0
            ),
            new CategoryDto(productGroup.Category.Id, productGroup.Category.Name),
            productGroup.Products
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
}
