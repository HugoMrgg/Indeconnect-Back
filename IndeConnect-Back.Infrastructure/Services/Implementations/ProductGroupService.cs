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

        // Vérifier que l'utilisateur est SuperVendor d'une marque
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (user == null || !user.BrandId.HasValue)
            throw new UnauthorizedAccessException("You must be a SuperVendor with a brand to create product groups.");

        var brandId = user.BrandId.Value;

        // Vérifier que la catégorie existe
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new InvalidOperationException($"Category with id {request.CategoryId} not found.");

        // Créer le ProductGroup
        var productGroup = new ProductGroup(
            name: request.Name,
            baseDescription: request.BaseDescription,
            brandId: brandId,
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

        // Vérifier que l'utilisateur est SuperVendor de cette marque
        if (productGroup.Brand.SuperVendorUserId != currentUserId)
            throw new UnauthorizedAccessException("You are not the SuperVendor of this brand.");

        // Vérifier que la catégorie existe
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new InvalidOperationException($"Category with id {request.CategoryId} not found.");

        // Mettre à jour (pas de méthode Update sur ProductGroup, on utilise les setters privés via reflection ou on ajoute une méthode Update)
        // Pour l'instant, je vais accéder directement aux propriétés (EF Core le permet)
        _context.Entry(productGroup).Property("Name").CurrentValue = request.Name.Trim();
        _context.Entry(productGroup).Property("BaseDescription").CurrentValue = request.BaseDescription.Trim();
        _context.Entry(productGroup).Property("CategoryId").CurrentValue = request.CategoryId;
        // Utiliser la méthode métier du Domain
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

    public async Task DeleteProductGroupAsync(long productGroupId, long? currentUserId)
    {
        var productGroup = await _context.ProductGroups
            .Include(pg => pg.Brand)
            .Include(pg => pg.Products)
            .FirstOrDefaultAsync(pg => pg.Id == productGroupId);

        if (productGroup == null)
            throw new KeyNotFoundException($"Product group with id {productGroupId} not found.");

        // Vérifier que l'utilisateur est SuperVendor de cette marque
        if (productGroup.Brand.SuperVendorUserId != currentUserId)
            throw new UnauthorizedAccessException("You are not the SuperVendor of this brand.");

        // Utiliser la méthode du Domain pour vérifier s'il y a des produits
        if (productGroup.HasProducts())
        // Vérifier qu'il n'y a pas de produits
        if (productGroup.Products.Any())
            throw new InvalidOperationException("Cannot delete a product group that contains products. Delete all products first.");

        _context.ProductGroups.Remove(productGroup);
        await _context.SaveChangesAsync();
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
            // Utiliser les méthodes du Domain
            productGroup.GetOnlineProducts()
            productGroup.Products
                .Where(p => p.IsEnabled && p.Status == ProductStatus.Online)
                .Select(p => new ProductColorVariantDto(
                    p.Id,
                    p.PrimaryColor?.Id,
                    p.PrimaryColor?.Name,
                    p.PrimaryColor?.Hexa,
                    p.GetPrimaryImageUrl(),
                    p.IsAvailableForPurchase()
                    p.Media.FirstOrDefault(m => m.IsPrimary)?.Url
                        ?? p.Media.OrderBy(m => m.DisplayOrder).FirstOrDefault()?.Url,
                    p.Variants.Sum(v => v.StockCount) > 0
                ))
        );
    }
}
