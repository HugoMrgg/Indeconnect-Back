﻿using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class BrandService : IBrandService
{
    private readonly AppDbContext _context;
    private readonly IGeocodeService _geocodeService;

    // TODO IMPORTANT: ces keys doivent matcher celles stockées en DB dans EthicsCategoryEntity.Key.
    // Je mets plusieurs alias pour être robuste si seed "creation" vs "materialsManufacturing", etc.
    private static readonly string[] ProductionCategoryKeys =
    {
        "materialsmanufacturing",
        "materials_manufacturing",
        "creation",
        "creation-des-habits",
        "production"
    };

    private static readonly string[] TransportCategoryKeys =
    {
        "transport"
    };
    
    // TODO
    // private const string TransportKey = "transport";
    // private const string CreationKey  = "creation";

    public BrandService(AppDbContext context, IGeocodeService geocodeService)
    {
        _context = context;
        _geocodeService = geocodeService;
    }

    public async Task<BrandsListResponse> GetBrandsSortedByEthicsAsync(GetBrandsQuery query)
    {
        var brandsQuery = _context.Brands
            .Where(b => b.Status == BrandStatus.Approved)
            .Include(b => b.EthicTags)
            .Include(b => b.Deposits)
            .Include(b => b.Reviews)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query.PriceRange))
        {
            brandsQuery = brandsQuery.Where(b => b.PriceRange == query.PriceRange);
        }

        if (query.EthicTags != null && query.EthicTags.Any())
        {
            foreach (var tag in query.EthicTags)
            {
                brandsQuery = brandsQuery.Where(b => b.EthicTags.Any(et => et.TagKey == tag));
            }
        }

        var brands = await brandsQuery.ToListAsync();

        // Charger en une fois les scores OFFICIELS persistés
        var scoresByBrand = await LoadOfficialEthicsScoresByBrandAsync(brands.Select(b => b.Id));

        var enrichedBrands = brands
            .Select(b =>
            {
                var ethicsScoreProduction = GetOfficialScoreByKeys(scoresByBrand, b.Id, ProductionCategoryKeys);
                var ethicsScoreTransportBase = GetOfficialScoreByKeys(scoresByBrand, b.Id, TransportCategoryKeys);

                var userRating = b.Reviews.Any() ? b.Reviews.Average(r => (double)r.Rating) : 0.0;

                var address = b.Deposits.FirstOrDefault() != null
                    ? $"{b.Deposits.First().Number} {b.Deposits.First().Street}, {b.Deposits.First().PostalCode}"
                    : null;

                var minDistance = query.Latitude.HasValue && query.Longitude.HasValue
                    ? GetMinimumDistanceToDeposits(b.Deposits, query.Latitude.Value, query.Longitude.Value)
                    : double.MaxValue;

                // Transport score = score officiel (depuis questionnaire approuvé) + multiplicateur "proximité utilisateur"
                var ethicsScoreTransport = ApplyUserDistanceMultiplierToTransport(
                    ethicsScoreTransportBase,
                    minDistance,
                    query.Latitude,
                    query.Longitude
                );

                return new
                {
                    Brand = b,
                    EthicsScoreProduction = ethicsScoreProduction,
                    EthicsScoreTransport = ethicsScoreTransport,
                    UserRating = userRating,
                    Address = address,
                    MinDistance = minDistance
                };
            })
            .ToList();

        if (query.UserRatingMin.HasValue)
        {
            enrichedBrands = enrichedBrands
                .Where(x => x.UserRating >= query.UserRatingMin.Value)
                .ToList();
        }

        if (query.MaxDistanceKm.HasValue && query.Latitude.HasValue && query.Longitude.HasValue)
        {
            enrichedBrands = enrichedBrands
                .Where(x => x.MinDistance <= query.MaxDistanceKm.Value)
                .ToList();
        }

        if (query.MinEthicsProduction.HasValue)
        {
            enrichedBrands = enrichedBrands
                .Where(x => x.EthicsScoreProduction >= query.MinEthicsProduction.Value)
                .ToList();
        }

        if (query.MinEthicsTransport.HasValue)
        {
            enrichedBrands = enrichedBrands
                .Where(x => x.EthicsScoreTransport >= query.MinEthicsTransport.Value)
                .ToList();
        }

        var sortedBrands = query.SortBy switch
        {
            EthicsSortType.Note => enrichedBrands.OrderByDescending(x => x.UserRating).ToList(),
            EthicsSortType.Distance => enrichedBrands.OrderBy(x => x.MinDistance).ToList(),
            EthicsSortType.Transport => enrichedBrands.OrderByDescending(x => x.EthicsScoreTransport).ToList(),
            _ => enrichedBrands.OrderByDescending(x => x.EthicsScoreProduction).ToList()
        };

        var totalCount = sortedBrands.Count;

        var paginatedBrands = sortedBrands
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => MapToBrandSummary(
                x.Brand,
                x.EthicsScoreProduction,
                x.EthicsScoreTransport,
                x.UserRating,
                x.Address,
                query.Latitude,
                query.Longitude
            ))
            .ToList();

        return new BrandsListResponse(
            paginatedBrands,
            totalCount,
            query.Page,
            query.PageSize,
            LocationUsed: query.Latitude.HasValue && query.Longitude.HasValue
        );
    }

    public async Task<BrandDetailDto?> GetBrandByIdAsync(long brandId, double? userLat, double? userLon)
    {
        var brand = await _context.Brands
            .Include(b => b.EthicTags)
            .Include(b => b.Deposits)
            .Include(b => b.Reviews)
            .FirstOrDefaultAsync(b => b.Id == brandId && b.Status == BrandStatus.Approved);

        if (brand == null)
            return null;

        var avgRating = brand.Reviews.Any() ? brand.Reviews.Average(r => (double)r.Rating) : 0.0;

        // Charger scores officiels (persistés) pour cette marque
        var scoresByBrand = await LoadOfficialEthicsScoresByBrandAsync(new[] { brand.Id });

        var ethicsScoreProduction = GetOfficialScoreByKeys(scoresByBrand, brand.Id, ProductionCategoryKeys);

        var transportBase = GetOfficialScoreByKeys(scoresByBrand, brand.Id, TransportCategoryKeys);
        var minDistance = userLat.HasValue && userLon.HasValue
            ? GetMinimumDistanceToDeposits(brand.Deposits, userLat.Value, userLon.Value)
            : double.MaxValue;

        var ethicsScoreTransport = ApplyUserDistanceMultiplierToTransport(transportBase, minDistance, userLat, userLon);

        var deposits = brand.Deposits.Select(d => new DepositDto(
            d.Id,
            d.GetFullAddress(),
            userLat.HasValue && userLon.HasValue
                ? (int?)CalculateDistanceKm(userLat.Value, userLon.Value, d.Latitude, d.Longitude)
                : null,
            d.City
        ));

        return new BrandDetailDto(
            brand.Id,
            brand.Name,
            brand.LogoUrl,
            brand.BannerUrl,
            brand.Description,
            brand.AboutUs,
            brand.WhereAreWe,
            brand.OtherInfo,
            brand.Contact,
            brand.PriceRange,
            Math.Round(avgRating, 1),
            brand.Reviews.Count,
            brand.EthicTags.Select(et => et.TagKey),
            deposits,
            Math.Round(ethicsScoreProduction, 2),
            brand.AccentColor
        );
    }

    /// <summary>
    /// Get the brand of the authenticated SuperVendor (for editing/preview)
    /// </summary>
    public async Task<BrandDetailDto?> GetMyBrandAsync(long? superVendorUserId)
    {
        // Récupérer l'utilisateur pour avoir son BrandId
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == superVendorUserId);

        if (user == null || !user.BrandId.HasValue)
            return null;

        // Récupérer SA marque (peu importe le Status)
        var brand = await _context.Brands
            .Include(b => b.EthicTags)
            .Include(b => b.Deposits)
            .Include(b => b.Reviews)
            .FirstOrDefaultAsync(b => b.Id == user.BrandId.Value);

        if (brand == null)
            return null;

        var avgRating = brand.Reviews.Any() ? brand.Reviews.Average(r => (double)r.Rating) : 0.0;

        // Scores officiels (si aucun questionnaire approuvé, ça retombe à 0)
        var scoresByBrand = await LoadOfficialEthicsScoresByBrandAsync(new[] { brand.Id });

        var ethicsScoreProduction = GetOfficialScoreByKeys(scoresByBrand, brand.Id, ProductionCategoryKeys);

        // Pas de userLat/userLon ici, donc pas de multiplicateur transport
        var ethicsScoreTransport = GetOfficialScoreByKeys(scoresByBrand, brand.Id, TransportCategoryKeys);

        var deposits = brand.Deposits.Select(d => new DepositDto(
            d.Id,
            d.GetFullAddress(),
            null,
            d.City
        ));

        return new BrandDetailDto(
            brand.Id,
            brand.Name,
            brand.LogoUrl,
            brand.BannerUrl,
            brand.Description,
            brand.AboutUs,
            brand.WhereAreWe,
            brand.OtherInfo,
            brand.Contact,
            brand.PriceRange,
            Math.Round(avgRating, 1),
            brand.Reviews.Count,
            brand.EthicTags.Select(et => et.TagKey),
            deposits,
            Math.Round(ethicsScoreProduction, 2),
            brand.AccentColor
        );
    }

    private double ApplyUserDistanceMultiplierToTransport(
        double transportBaseScore,
        double minDistanceKm,
        double? userLat,
        double? userLon)
    {
        // Comme avant : le multiplicateur est "UX" (proximité à l'utilisateur),
        // donc il doit rester calculé à la volée.
        if (!userLat.HasValue || !userLon.HasValue)
            return transportBaseScore;

        var distanceMultiplier = minDistanceKm switch
        {
            < 50 => 2.0,
            < 200 => 1.5,
            < 500 => 1.0,
            _ => 0.5
        };

        return transportBaseScore * distanceMultiplier;
    }

    private double GetMinimumDistanceToDeposits(
        IEnumerable<Deposit> deposits,
        double userLat,
        double userLon)
    {
        if (!deposits.Any())
            return double.MaxValue;

        var validDeposits = deposits
            .Where(d => d.Latitude != 0 && d.Longitude != 0)
            .ToList();

        if (!validDeposits.Any())
            return double.MaxValue;

        var distances = validDeposits
            .Select(d => CalculateDistanceKm(userLat, userLon, d.Latitude, d.Longitude))
            .ToList();

        return distances.Min();
    }

    private double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private BrandSummaryDto MapToBrandSummary(
        Brand brand,
        double ethicsScoreProduction,
        double ethicsScoreTransport,
        double userRating,
        string? address,
        double? userLat,
        double? userLon)
    {
        int? distanceKm = null;
        if (userLat.HasValue && userLon.HasValue && brand.Deposits.Any())
        {
            distanceKm = (int)GetMinimumDistanceToDeposits(
                brand.Deposits,
                userLat.Value,
                userLon.Value
            );
        }

        return new BrandSummaryDto(
            brand.Id,
            brand.Name,
            brand.LogoUrl,
            brand.BannerUrl,
            brand.Description,
            ethicsScoreProduction,
            ethicsScoreTransport,
            brand.EthicTags.Select(et => et.TagKey),
            address,
            distanceKm,
            Math.Round(userRating, 1)
        );
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180;

    public async Task UpdateBrandAsync(long brandId, UpdateBrandRequest request, long? currentUserId)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == brandId);

        if (brand == null)
            throw new KeyNotFoundException($"Brand with ID {brandId} not found");

        if (!brand.SuperVendorUserId.HasValue || brand.SuperVendorUserId.Value != currentUserId)
            throw new UnauthorizedAccessException("You are not allowed to modify this brand.");

        brand.UpdateGeneralInfo(
            request.Name,
            request.LogoUrl,
            request.BannerUrl,
            request.Description,
            request.AboutUs,
            request.WhereAreWe,
            request.OtherInfo,
            request.Contact,
            request.PriceRange,
            request.AccentColor
        );

        await _context.SaveChangesAsync();
    }

    public async Task<DepositDto> UpsertMyBrandDepositAsync(
        long? currentUserId,
        UpsertBrandDepositRequest request)
    {
        var brand = await _context.Brands
            .Include(b => b.Deposits)
            .FirstOrDefaultAsync(b => b.SuperVendorUserId == currentUserId);

        if (brand == null)
            throw new KeyNotFoundException("No brand associated with this user.");

        var existing = brand.Deposits.FirstOrDefault();
        var id = existing?.Id ?? Guid.NewGuid().ToString("N");

        // Adresse complète pour le geocoding
        var fullAddress =
            $"{request.Number} {request.Street}, {request.PostalCode} {request.City}, {request.Country}";

        // Appel au service de geocoding (NominatimGeocodeService)
        var coords = await _geocodeService.GeocodeAddressAsync(fullAddress);

        var latitude = coords?.Latitude ?? 0;
        var longitude = coords?.Longitude ?? 0;

        brand.SetSingleDeposit(
            id: id,
            number: request.Number,
            street: request.Street,
            postalCode: request.PostalCode,
            city: request.City,
            country: request.Country,
            latitude: latitude,
            longitude: longitude
        );

        await _context.SaveChangesAsync();

        var deposit = brand.Deposits.First();

        return new DepositDto(
            deposit.Id,
            deposit.GetFullAddress(),
            null,
            deposit.City
        );
    }

    // -------------------------
    // Scores officiels persistés
    // -------------------------

    private async Task<Dictionary<long, Dictionary<string, double>>> LoadOfficialEthicsScoresByBrandAsync(IEnumerable<long> brandIds)
    {
        var ids = brandIds.Distinct().ToList();
        if (ids.Count == 0) return new();

        // OFFICIEL uniquement : provient du dernier questionnaire Approved (après review admin)
        var rows = await _context.BrandEthicScores
            .AsNoTracking()
            .Include(s => s.Category)
            .Where(s => s.IsOfficial && ids.Contains(s.BrandId))
            .Select(s => new
            {
                s.BrandId,
                CategoryKey = s.Category.Key,
                FinalScore = (double)s.FinalScore
            })
            .ToListAsync();

        var dict = new Dictionary<long, Dictionary<string, double>>();

        foreach (var r in rows)
        {
            if (!dict.TryGetValue(r.BrandId, out var byCat))
            {
                byCat = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                dict[r.BrandId] = byCat;
            }

            byCat[r.CategoryKey] = r.FinalScore;
        }

        return dict;
    }

    private static double GetOfficialScoreByKeys(
        Dictionary<long, Dictionary<string, double>> scoresByBrand,
        long brandId,
        IEnumerable<string> possibleKeys)
    {
        if (!scoresByBrand.TryGetValue(brandId, out var byCat))
            return 0.0;

        foreach (var key in possibleKeys)
        {
            if (byCat.TryGetValue(key, out var score))
                return score;
        }

        return 0.0;
    }
}
