namespace IndeConnect_Back.Application.DTOs.Brands;

public record GetBrandsQuery(
    EthicsSortType SortBy = EthicsSortType.MaterialsManufacturing,
    double? Latitude = null,
    double? Longitude = null,
    int Page = 1,
    int PageSize = 20,
    string? PriceRange = null,    
    double? UserRatingMin = null, 
    double? MaxDistanceKm = null,
    double? MinEthicsProduction = null,  
    double? MinEthicsTransport = null,
    IEnumerable<string>? EthicTags = null
);

public enum EthicsSortType
{
    MaterialsManufacturing,
    Transport,
    Distance,
    Note
}