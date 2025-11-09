namespace IndeConnect_Back.Application.DTOs.Brands;

public record BrandsListResponse(
    IEnumerable<BrandSummaryDto> Brands,
    int TotalCount,
    int Page,
    int PageSize,
    bool LocationUsed
);