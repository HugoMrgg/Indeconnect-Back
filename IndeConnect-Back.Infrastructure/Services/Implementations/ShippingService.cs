using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class ShippingService : IShippingService
{
    private readonly AppDbContext _context;

    public ShippingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ShippingMethodDto>> GetBrandShippingMethodsAsync(long brandId)
    {
        var methods = await _context.Set<BrandShippingMethod>()
            .Where(m => m.BrandId == brandId && m.IsEnabled)
            .OrderBy(m => m.Price)
            .ThenBy(m => m.DisplayName)
            .ToListAsync();

        return methods.Select(MapToDto).ToList();
    }

    public async Task<ShippingMethodDto> CreateBrandShippingMethodAsync(long brandId, CreateShippingMethodDto dto)
    {
        // Vérifier que la marque existe
        var brandExists = await _context.Set<Brand>().AnyAsync(b => b.Id == brandId);
        if (!brandExists)
            throw new InvalidOperationException($"La marque {brandId} n'existe pas");

        // Parser le MethodType
        if (!Enum.TryParse<ShippingMethodType>(dto.MethodType, out var methodType))
            throw new ArgumentException($"Type de méthode invalide: {dto.MethodType}");

        // Créer la méthode
        var method = new BrandShippingMethod(
            brandId: brandId,
            providerName: dto.ProviderName,
            methodType: methodType,
            displayName: dto.DisplayName,
            price: dto.Price,
            estimatedMinDays: dto.EstimatedMinDays,
            estimatedMaxDays: dto.EstimatedMaxDays,
            maxWeight: dto.MaxWeight,
            providerConfig: dto.ProviderConfig
        );

        _context.Set<BrandShippingMethod>().Add(method);
        await _context.SaveChangesAsync();

        return MapToDto(method);
    }

    public async Task<ShippingMethodDto> UpdateBrandShippingMethodAsync(long brandId, long methodId, UpdateShippingMethodDto dto)
    {
        var method = await _context.Set<BrandShippingMethod>()
            .FirstOrDefaultAsync(m => m.Id == methodId && m.BrandId == brandId);

        if (method == null)
            throw new InvalidOperationException($"Méthode {methodId} introuvable pour la marque {brandId}");

        // Parser le MethodType si fourni
        ShippingMethodType? methodType = null;
        if (!string.IsNullOrEmpty(dto.MethodType))
        {
            if (!Enum.TryParse<ShippingMethodType>(dto.MethodType, out var parsedType))
                throw new ArgumentException($"Type de méthode invalide: {dto.MethodType}");
            methodType = parsedType;
        }

        // Mettre à jour
        method.Update(
            providerName: dto.ProviderName,
            methodType: methodType,
            displayName: dto.DisplayName,
            price: dto.Price,
            estimatedMinDays: dto.EstimatedMinDays,
            estimatedMaxDays: dto.EstimatedMaxDays,
            maxWeight: dto.MaxWeight,
            providerConfig: dto.ProviderConfig
        );

        if (dto.IsEnabled.HasValue)
        {
            if (dto.IsEnabled.Value)
                method.Enable();
            else
                method.Disable();
        }

        await _context.SaveChangesAsync();

        return MapToDto(method);
    }

    public async Task DeleteBrandShippingMethodAsync(long brandId, long methodId)
    {
        var method = await _context.Set<BrandShippingMethod>()
            .FirstOrDefaultAsync(m => m.Id == methodId && m.BrandId == brandId);

        if (method == null)
            throw new InvalidOperationException($"Méthode {methodId} introuvable pour la marque {brandId}");

        _context.Set<BrandShippingMethod>().Remove(method);
        await _context.SaveChangesAsync();
    }

    private static ShippingMethodDto MapToDto(BrandShippingMethod method)
    {
        return new ShippingMethodDto
        {
            Id = method.Id,
            ProviderName = method.ProviderName,
            MethodType = method.MethodType.ToString(),
            DisplayName = method.DisplayName,
            Price = method.Price,
            EstimatedMinDays = method.EstimatedMinDays,
            EstimatedMaxDays = method.EstimatedMaxDays,
            MaxWeight = method.MaxWeight,
            IsEnabled = method.IsEnabled
        };
    }
}