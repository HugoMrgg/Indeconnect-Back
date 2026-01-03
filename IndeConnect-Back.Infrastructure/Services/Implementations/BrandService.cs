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
                var ethicsScoreProduction = GetOfficialScoreByKeys(scoresByBrand, b.Id, EthicsCategoryKeys.Production);
                var ethicsScoreTransportBase = GetOfficialScoreByKeys(scoresByBrand, b.Id, EthicsCategoryKeys.Transport);

                var userRating = b.GetAverageRating();

                var address = b.Deposits.FirstOrDefault() != null
                    ? $"{b.Deposits.First().Number} {b.Deposits.First().Street}, {b.Deposits.First().PostalCode}"
                    : null;

                var minDistance = query.Latitude.HasValue && query.Longitude.HasValue
                    ? b.GetClosestDepositDistance(query.Latitude.Value, query.Longitude.Value)
                    : double.MaxValue;

                // Transport score = score officiel (depuis questionnaire approuvé) + multiplicateur "proximité utilisateur"
                var ethicsScoreTransport = EthicsDistanceMultiplier.ApplyToScore(
                    ethicsScoreTransportBase,
                    minDistance != double.MaxValue ? minDistance : null
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

        return await BuildBrandDetailDtoAsync(brand, userLat, userLon);
    }

    /// <summary>
    /// Get the brand of the authenticated SuperVendor (for editing/preview)
    /// </summary>
    public async Task<BrandDetailDto?> GetMyBrandAsync(long? userId)
    {
        if (!userId.HasValue)
            return null;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        Brand? brand = null;

        // SuperVendor : a un BrandId direct
        if (user.BrandId.HasValue)
        {
            brand = await _context.Brands
                .Include(b => b.EthicTags)
                .Include(b => b.Deposits)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == user.BrandId.Value);
        }
        // Vendor : chercher via BrandSellers directement depuis le DbSet
        else
        {

            var activeBrandSeller = await _context.BrandSellers
                .Where(bs => bs.SellerId == userId && bs.IsActive)
                .FirstOrDefaultAsync();

            if (activeBrandSeller != null)
            {
                brand = await _context.Brands
                    .Include(b => b.EthicTags)
                    .Include(b => b.Deposits)
                    .Include(b => b.Reviews)
                    .FirstOrDefaultAsync(b => b.Id == activeBrandSeller.BrandId);
            }
        }

        if (brand == null)
            return null;

        return await BuildBrandDetailDtoAsync(brand, userLat: null, userLon: null);
    }


    /// <summary>
    /// Construit un BrandDetailDto à partir d'une entité Brand.
    /// Applique le multiplicateur de distance au score Transport si userLat/userLon sont fournis.
    /// </summary>
    private async Task<BrandDetailDto> BuildBrandDetailDtoAsync(
        Brand brand,
        double? userLat,
        double? userLon)
    {
        var avgRating = brand.GetAverageRating();

        // Charger scores officiels (persistés) pour cette marque
        var scoresByBrand = await LoadOfficialEthicsScoresByBrandAsync(new[] { brand.Id });

        var ethicsScoreProduction = GetOfficialScoreByKeys(scoresByBrand, brand.Id, EthicsCategoryKeys.Production);

        var transportBase = GetOfficialScoreByKeys(scoresByBrand, brand.Id, EthicsCategoryKeys.Transport);
        var minDistance = userLat.HasValue && userLon.HasValue
            ? brand.GetClosestDepositDistance(userLat.Value, userLon.Value)
            : double.MaxValue;

        var ethicsScoreTransport = EthicsDistanceMultiplier.ApplyToScore(
            transportBase,
            minDistance != double.MaxValue ? minDistance : null
        );

        var deposits = brand.Deposits.Select(d => new DepositDto(
            d.Id,
            d.GetFullAddress(),
            userLat.HasValue && userLon.HasValue
                ? (int?)GeographicDistance.CalculateKm(userLat.Value, userLon.Value, d.Latitude, d.Longitude)
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
            Math.Round(ethicsScoreTransport, 2),
            brand.AccentColor
        );
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
            var distance = brand.GetClosestDepositDistance(userLat.Value, userLon.Value);
            distanceKm = distance != double.MaxValue ? (int)distance : null;
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
            .Where(s => s.IsOfficial && ids.Contains(s.BrandId))
            .Select(s => new
            {
                s.BrandId,
                CategoryKey = s.Category.ToString(),
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
