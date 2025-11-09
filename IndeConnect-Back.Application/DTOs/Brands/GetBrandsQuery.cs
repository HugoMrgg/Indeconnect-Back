namespace IndeConnect_Back.Application.DTOs.Brands;

public record GetBrandsQuery(
    EthicsSortType SortBy,
    double? Latitude = null,      
    double? Longitude = null,
    int Page = 1,
    int PageSize = 20
);

public enum EthicsSortType
{
    MaterialsManufacturing,
    Transport
}