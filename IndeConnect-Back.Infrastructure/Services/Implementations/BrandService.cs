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
        // Get active brands with ethics data
        var brands = await _context.Brands
            .Where(b => b.Status == BrandStatus.Approved)
            .Include(b => b.EthicTags)
            .Include(b => b.Deposits)
            .ToListAsync();

        bool locationUsed = query.Latitude.HasValue && query.Longitude.HasValue;

        // Calculate scores
        var scoredBrands = brands
            .Select(b => new
            {
                Brand = b,
                Score = CalculateEthicsScore(b, query.SortBy, query.Latitude, query.Longitude)
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        var totalCount = scoredBrands.Count;

        // Paginate
        var paginatedBrands = scoredBrands
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => MapToBrandSummary(x.Brand, x.Score, query.Latitude, query.Longitude))
            .ToList();

        return new BrandsListResponse(
            paginatedBrands,
            totalCount,
            query.Page,
            query.PageSize,
            locationUsed
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

            var distanceMultiplier = minDistance switch {
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
        if (!deposits.Any()) return double.MaxValue;

        var distances = deposits
            .Select(d => CalculateDistanceKm(
                userLat, userLon,
                d.Latitude, d.Longitude  
            ))
            .ToList();

        return distances.Any() ? distances.Min() : double.MaxValue;
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
        double score,
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
            Math.Round(score, 2),
            brand.EthicTags.Select(et => et.TagKey),
            distanceKm
        );
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180;
}
