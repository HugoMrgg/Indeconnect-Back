using IndeConnect_Back.Application.DTOs.Users;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IShippingAddressService
{
    Task<IEnumerable<ShippingAddressDto>> GetUserAddressesAsync(long userId);
    Task<ShippingAddressDto?> GetAddressByIdAsync(long addressId);
    Task<ShippingAddressDto> CreateAddressAsync(long userId, CreateShippingAddressDto dto);
    Task<ShippingAddressDto?> UpdateAddressAsync(long addressId, long userId, UpdateShippingAddressDto dto);
    Task<bool> DeleteAddressAsync(long addressId, long userId);
    Task<ShippingAddressDto?> SetDefaultAddressAsync(long addressId, long userId);

}