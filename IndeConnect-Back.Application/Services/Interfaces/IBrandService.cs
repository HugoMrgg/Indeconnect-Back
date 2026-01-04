using IndeConnect_Back.Application.DTOs.Brands;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IBrandService
{
    Task<BrandsListResponse> GetBrandsSortedByEthicsAsync(GetBrandsQuery query);
    Task<BrandDetailDto?> GetBrandByIdAsync(long brandId, double? userLat, double? userLon);
    Task<BrandDetailDto?> GetMyBrandAsync(long? superVendorUserId);
    Task UpdateBrandAsync(long brandId, UpdateBrandRequest request, long? currentUserId);
    Task<DepositDto> UpsertMyBrandDepositAsync(long? currentUserId, UpsertBrandDepositRequest request);
    Task<IEnumerable<BrandModerationListDto>> GetBrandsForModerationAsync();
    Task<BrandModerationDetailDto?> GetBrandForModerationAsync(long brandId);
    Task SubmitBrandAsync(long brandId, long superVendorUserId);
    Task ApproveBrandAsync(long brandId, long moderatorUserId);
    Task RejectBrandAsync(long brandId, long moderatorUserId, string reason);
}