using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class BrandService : IBrandService
{
    private readonly AppDbContext _context;
    private readonly BrandEthicsScorer _ethicsScorer;
    
    public BrandService(AppDbContext context, BrandEthicsScorer ethicsScorer)
    {
        _context = context;
        _ethicsScorer = ethicsScorer;
        }
    public async Task<BrandsListResponse> GetBrandsSortedByEthicsAsync(GetBrandsQuery query)
    {
        var brandsQuery = _context.Brands
            .Where(b => b.Status == BrandStatus.Approved)
            .Include(b => b.EthicTags)
            .Include(b => b.Deposits)
            .Include(b => b.Reviews)
            .Include(b => b.Questionnaires)
                .ThenInclude(q => q.Responses)
                    .ThenInclude(r => r.Option)
            .Include(b => b.Questionnaires)
                .ThenInclude(q => q.Responses)
                    .ThenInclude(r => r.Question)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query.PriceRange))
        {
            brandsQuery = brandsQuery.Where(b => b.PriceRange == query.PriceRange);
        }

        // ✅ NOUVEAU : Filtre par tags éthiques (ET logique)
        if (query.EthicTags != null && query.EthicTags.Any())
        {
            foreach (var tag in query.EthicTags)
            {
                // Chaque tag doit être présent (ET logique)
                brandsQuery = brandsQuery.Where(b => b.EthicTags.Any(et => et.TagKey == tag));
            }
        }

        var brands = await brandsQuery.ToListAsync();

        var enrichedBrands = brands
            .Select(b => new
            {
                Brand = b,
                EthicsScoreProduction = CalculateEthicsScore(b, EthicsSortType.MaterialsManufacturing, query.Latitude, query.Longitude),
                EthicsScoreTransport = CalculateEthicsScore(b, EthicsSortType.Transport, query.Latitude, query.Longitude),
                UserRating = b.Reviews.Any() ? b.Reviews.Average(r => (double)r.Rating) : 0.0,
                Address = b.Deposits.FirstOrDefault() != null
                    ? $"{b.Deposits.First().Number} {b.Deposits.First().Street}, {b.Deposits.First().PostalCode}"
                    : null,
                MinDistance = query.Latitude.HasValue && query.Longitude.HasValue
                    ? GetMinimumDistanceToDeposits(b.Deposits, query.Latitude.Value, query.Longitude.Value)
                    : double.MaxValue
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

        // Tri
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
            .Include(b => b.Questionnaires)
            .ThenInclude(q => q.Responses)
            .ThenInclude(r => r.Option)
            .FirstOrDefaultAsync(b => b.Id == brandId && b.Status == BrandStatus.Approved);

        if (brand == null)
            return null;

        var avgRating = brand.Reviews.Any() ? brand.Reviews.Average(r => (double)r.Rating) : 0.0;
        var ethicsScoreProduction = CalculateEthicsScore(brand, EthicsSortType.MaterialsManufacturing, userLat, userLon);
        var ethicsScoreTransport = CalculateEthicsScore(brand, EthicsSortType.Transport, userLat, userLon);

        var deposits = brand.Deposits.Select(d => new DepositDto(
            d.Id,
            d.GetFullAddress(),
            userLat.HasValue && userLon.HasValue 
                ? (int?)CalculateDistanceKm(userLat.Value, userLon.Value, d.Latitude, d.Longitude)
                : null
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
            Math.Round(ethicsScoreProduction, 2) 
        );
    }

    private double CalculateEthicsScore(
        Brand brand,
        EthicsSortType sortBy,
        double? userLat,
        double? userLon)
    {
        var category = sortBy == EthicsSortType.MaterialsManufacturing
            ? EthicsCategory.MaterialsManufacturing
            : EthicsCategory.Transport;

        var questionnaireResponses = brand.Questionnaires.SelectMany(q => q.Responses);
        decimal baseScore = _ethicsScorer.ComputeScore(questionnaireResponses, category);

        if (sortBy == EthicsSortType.Transport && userLat.HasValue && userLon.HasValue)
        {
            var minDistance = GetMinimumDistanceToDeposits(
                brand.Deposits,
                userLat.Value,
                userLon.Value
            );

            var distanceMultiplier = minDistance switch
            {
                < 50 => 2.0m,
                < 200 => 1.5m,
                < 500 => 1.0m,
                _ => 0.5m
            };
            
            baseScore *= distanceMultiplier;
        }
        
        return (double)baseScore;
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
}
