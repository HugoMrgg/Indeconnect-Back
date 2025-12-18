using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class ShippingService : IShippingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(AppDbContext context, ILogger<ShippingService> logger)
    {
        _context = context;
        _logger = logger;
    }

   public async Task<List<ShippingMethodDto>> GetBrandShippingMethodsAsync(long brandId, long? shippingAddressId = null)
{
    var methods = await _context.Set<BrandShippingMethod>()
        .Where(m => m.BrandId == brandId && m.IsEnabled)
        .OrderBy(m => m.Price)
        .ThenBy(m => m.DisplayName)
        .ToListAsync();


    // Si pas d'adresse fournie, retourner les méthodes sans calcul
    if (!shippingAddressId.HasValue)
    {
        return methods.Select(MapToDto).ToList();
    }


    // Charger l'adresse de livraison
    var shippingAddress = await _context.ShippingAddresses
        .FirstOrDefaultAsync(a => a.Id == shippingAddressId.Value);

    if (shippingAddress == null)
    {
        return methods.Select(MapToDto).ToList();
    }


    // Charger le premier dépôt de la marque
    var deposit = await _context.Deposits
        .Where(d => d.BrandId == brandId)
        .FirstOrDefaultAsync();

    if (deposit == null)
    {
        return methods.Select(MapToDto).ToList();
    }

    // Calculer avec la MÊME logique que OrderService
    var now = DateTimeOffset.UtcNow;
    
    // Mapper avec les calculs
    var result = methods.Select(m => MapToDtoWithCalculation(m, deposit, shippingAddress, now)).ToList();
    
    return result;
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

    /// <summary>
    /// Mapper avec calculs basés sur deposit + shippingAddress + shippingMethod
    /// </summary>
    private ShippingMethodDto MapToDtoWithCalculation(
        BrandShippingMethod method,
        Deposit deposit,
        ShippingAddress shippingAddress,
        DateTimeOffset now)
    {
        // Utiliser le DeliveryEstimator du domaine pour calculer la date estimée
        var estimatedDate = DeliveryEstimator.CalculateEstimatedDeliveryDate(deposit, shippingAddress, now, method);

        // Calculer les délais totaux en jours à partir de maintenant
        var totalDays = (estimatedDate - now).TotalDays;
        var totalMinDays = (int)Math.Floor(totalDays);
        var totalMaxDays = (int)Math.Ceiling(totalDays);

        _logger.LogInformation(
            "Calculated delivery for method {MethodId}: {EstimatedDate} (Total: {TotalDays} days)",
            method.Id, estimatedDate, totalDays);

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
            IsEnabled = method.IsEnabled,
            TotalEstimatedMinDays = totalMinDays,
            TotalEstimatedMaxDays = totalMaxDays,
            EstimatedDeliveryDate = estimatedDate
        };
    }
}
