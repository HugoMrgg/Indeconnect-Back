using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class ShippingAddressService : IShippingAddressService
{
    private readonly AppDbContext _context;

    public ShippingAddressService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ShippingAddressDto>> GetUserAddressesAsync(long userId)
    {
        var addresses = await _context.ShippingAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.Id)
            .ToListAsync();

        return addresses.Select(MapToDto);
    }

    public async Task<ShippingAddressDto?> GetAddressByIdAsync(long addressId)
    {
        var address = await _context.ShippingAddresses.FindAsync(addressId);
        return address == null ? null : MapToDto(address);
    }

    public async Task<ShippingAddressDto> CreateAddressAsync(long userId, CreateShippingAddressDto dto)
    {
        if (dto.IsDefault)
        {
            await UnsetAllDefaultAddressesAsync(userId);
        }

        var address = new ShippingAddress(
            userId: userId,
            street: dto.Street,
            number: dto.Number,
            postalCode: dto.PostalCode,
            city: dto.City,
            country: dto.Country,
            isDefault: dto.IsDefault,
            extra: dto.Extra
        );

        _context.ShippingAddresses.Add(address);
        await _context.SaveChangesAsync();

        return MapToDto(address);
    }

    public async Task<ShippingAddressDto?> UpdateAddressAsync(long addressId, long userId, UpdateShippingAddressDto dto)
    {
        var address = await _context.ShippingAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

        if (address == null)
        {
            return null;
        }

        if (dto.IsDefault == true && !address.IsDefault)
        {
            await UnsetAllDefaultAddressesAsync(userId);
            address.SetAsDefault();
        }
        else if (dto.IsDefault == false && address.IsDefault)
        {
            address.UnsetAsDefault();
        }

        address.Update(
            street: dto.Street,
            number: dto.Number,
            postalCode: dto.PostalCode,
            city: dto.City,
            country: dto.Country,
            extra: dto.Extra
        );

        await _context.SaveChangesAsync();

        return MapToDto(address);
    }

    public async Task<bool> DeleteAddressAsync(long addressId, long userId)
    {
        var address = await _context.ShippingAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

        if (address == null)
        {
            return false;
        }

        _context.ShippingAddresses.Remove(address);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ShippingAddressDto?> SetDefaultAddressAsync(long addressId, long userId)
    {
        var address = await _context.ShippingAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

        if (address == null)
        {
            return null;
        }

        await UnsetAllDefaultAddressesAsync(userId);
        
        address.SetAsDefault();
        await _context.SaveChangesAsync();

        return MapToDto(address);
    }


    private async Task UnsetAllDefaultAddressesAsync(long userId)
    {
        var defaultAddresses = await _context.ShippingAddresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ToListAsync();

        foreach (var addr in defaultAddresses)
        {
            addr.UnsetAsDefault();
        }

        await _context.SaveChangesAsync();
    }

    private static ShippingAddressDto MapToDto(ShippingAddress address)
    {
        return new ShippingAddressDto
        {
            Id = address.Id,
            UserId = address.UserId,
            Street = address.Street,
            Number = address.Number,
            PostalCode = address.PostalCode,
            City = address.City,
            Country = address.Country,
            Extra = address.Extra,
            IsDefault = address.IsDefault
        };
    }
}