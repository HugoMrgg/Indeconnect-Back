using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/users/{userId}/shipping-addresses")]
[Authorize]
public class ShippingAddressController : ControllerBase
{
    private readonly IShippingAddressService _shippingAddressService;

    public ShippingAddressController(IShippingAddressService shippingAddressService)
    {
        _shippingAddressService = shippingAddressService;
    }

    /// <summary>
    /// GET /api/users/{userId}/shipping-addresses
    /// Récupère toutes les adresses de livraison d'un utilisateur
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShippingAddressDto>>> GetUserAddresses(long userId)
    {
        var addresses = await _shippingAddressService.GetUserAddressesAsync(userId);
        return Ok(addresses);
    }

    /// <summary>
    /// GET /api/users/{userId}/shipping-addresses/{addressId}
    /// Récupère une adresse spécifique
    /// </summary>
    [HttpGet("{addressId}")]
    public async Task<ActionResult<ShippingAddressDto>> GetAddress(long userId, long addressId)
    {
        var address = await _shippingAddressService.GetAddressByIdAsync(addressId);

        if (address == null || address.UserId != userId)
        {
            return NotFound("Adresse non trouvée");
        }

        return Ok(address);
    }

    /// <summary>
    /// POST /api/users/{userId}/shipping-addresses
    /// Crée une nouvelle adresse de livraison
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ShippingAddressDto>> CreateAddress(
        long userId,
        [FromBody] CreateShippingAddressDto dto)
    {
        var address = await _shippingAddressService.CreateAddressAsync(userId, dto);
        return CreatedAtAction(nameof(GetAddress), new { userId, addressId = address.Id }, address);
    }

    /// <summary>
    /// PUT /api/users/{userId}/shipping-addresses/{addressId}
    /// Modifie une adresse existante
    /// </summary>
    [HttpPut("{addressId}")]
    public async Task<ActionResult<ShippingAddressDto>> UpdateAddress(
        long userId,
        long addressId,
        [FromBody] UpdateShippingAddressDto dto)
    {
        var address = await _shippingAddressService.UpdateAddressAsync(addressId, userId, dto);

        if (address == null)
        {
            return NotFound("Adresse non trouvée");
        }

        return Ok(address);
    }

    /// <summary>
    /// DELETE /api/users/{userId}/shipping-addresses/{addressId}
    /// Supprime une adresse de livraison
    /// </summary>
    [HttpDelete("{addressId}")]
    public async Task<IActionResult> DeleteAddress(long userId, long addressId)
    {
        var success = await _shippingAddressService.DeleteAddressAsync(addressId, userId);

        if (!success)
        {
            return NotFound("Adresse non trouvée");
        }

        return NoContent();
    }

    /// <summary>
    /// PATCH /api/users/{userId}/shipping-addresses/{addressId}/set-default
    /// Définit une adresse comme adresse par défaut
    /// </summary>
    [HttpPatch("{addressId}/set-default")]
    public async Task<ActionResult<ShippingAddressDto>> SetDefaultAddress(long userId, long addressId)
    {
        var address = await _shippingAddressService.SetDefaultAddressAsync(addressId, userId);

        if (address == null)
        {
            return NotFound("Adresse non trouvée");
        }

        return Ok(address);
    }
}
