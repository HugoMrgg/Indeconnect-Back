using IndeConnect_Back.Application.DTOs.Brands;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IShippingService
{
    Task<List<ShippingMethodDto>> GetBrandShippingMethodsAsync(long brandId, long? shippingAddressId = null);
    Task<ShippingMethodDto> CreateBrandShippingMethodAsync(long brandId, CreateShippingMethodDto dto);
    Task<ShippingMethodDto> UpdateBrandShippingMethodAsync(long brandId, long methodId, UpdateShippingMethodDto dto);
    Task DeleteBrandShippingMethodAsync(long brandId, long methodId);
}