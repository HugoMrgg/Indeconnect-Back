using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect")]
public class ShippingController : ControllerBase
{
    private readonly IShippingService _shippingService;

    public ShippingController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    /// <summary>
    /// Récupère les méthodes de livraison disponibles pour une marque
    /// Utilisé par les clients dans le checkout
    /// </summary>
    [HttpGet("shipping/brands/{brandId}/methods")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ShippingMethodDto>>> GetBrandShippingMethods(
        long brandId,
        [FromQuery] long? addressId = null)
    {
        try
        {
            var methods = await _shippingService.GetBrandShippingMethodsAsync(brandId, addressId);
            return Ok(methods);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Crée une nouvelle méthode de livraison pour une marque
    /// Réservé au SuperVendor de la marque
    /// </summary>
    [HttpPost("brands/{brandId}/shipping-methods")]
    [Authorize(Roles = "SuperVendor")]
    public async Task<ActionResult<ShippingMethodDto>> CreateShippingMethod(
        long brandId,
        [FromBody] CreateShippingMethodDto dto)
    {
        try
        {
            // TODO: Vérifier que l'utilisateur est bien le SuperVendor de cette marque
            
            var method = await _shippingService.CreateBrandShippingMethodAsync(brandId, dto);
            return CreatedAtAction(nameof(GetBrandShippingMethods), new { brandId }, method);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Met à jour une méthode de livraison existante
    /// Réservé au SuperVendor de la marque
    /// </summary>
    [HttpPut("brands/{brandId}/shipping-methods/{methodId}")]
    [Authorize(Roles = "SuperVendor")]
    public async Task<ActionResult<ShippingMethodDto>> UpdateShippingMethod(
        long brandId,
        long methodId,
        [FromBody] UpdateShippingMethodDto dto)
    {
        try
        {
            // TODO: Vérifier que l'utilisateur est bien le SuperVendor de cette marque
            
            var method = await _shippingService.UpdateBrandShippingMethodAsync(brandId, methodId, dto);
            return Ok(method);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Supprime une méthode de livraison
    /// Réservé au SuperVendor de la marque
    /// </summary>
    [HttpDelete("brands/{brandId}/shipping-methods/{methodId}")]
    [Authorize(Roles = "SuperVendor")]
    public async Task<ActionResult> DeleteShippingMethod(long brandId, long methodId)
    {
        try
        {
            // TODO: Vérifier que l'utilisateur est bien le SuperVendor de cette marque
            
            await _shippingService.DeleteBrandShippingMethodAsync(brandId, methodId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
