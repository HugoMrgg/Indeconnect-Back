using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/users/{userId}/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly UserHelper _userHelper;

    public CartController(ICartService cartService, UserHelper userHelper)
    {
        _cartService = cartService;
        _userHelper  = userHelper;
    }

    /// <summary>
    /// Récupère le panier de l'utilisateur.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CartDto>> GetCart([FromRoute] long userId)
    {
        var currentUserId = _userHelper.GetUserId();
        if (currentUserId != userId && !_userHelper.IsAdminOrModerator())
            return Forbid();

        var cart = await _cartService.GetUserCartAsync(userId);
        return Ok(cart);
    }

    /// <summary>
    /// Ajoute une variante de produit au panier de l'utilisateur.
    /// Route : POST /indeconnect/users/{userId}/cart/variant/{variantId}
    /// </summary>
    [HttpPost("variant/{variantId:long}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> AddVariantToCart(
        [FromRoute] long userId,
        [FromRoute] long variantId,
        [FromBody] AddToCartRequest request)
    {
        var currentUserId = _userHelper.GetUserId();
        if (currentUserId != userId && !_userHelper.IsAdminOrModerator())
            return Forbid();

        if (request == null || request.Quantity <= 0)
        {
            return BadRequest(new { message = "Quantity must be greater than zero" });
        }

        var cart = await _cartService.AddVariantToCartAsync(userId, variantId, request.Quantity);

        if (cart == null)
            return NotFound(new { message = "Variant not found" });

        return Ok(cart);
    }
    /// <summary>
    /// Retire une variante du panier ou diminue sa quantité.
    /// Route : DELETE /indeconnect/users/{userId}/cart/variant/{variantId}
    /// </summary>
    [HttpDelete("variant/{variantId:long}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> RemoveVariantFromCart(
        [FromRoute] long userId,
        [FromRoute] long variantId,
        [FromQuery] int? quantity = null) // Si null, retire complètement
    {
        var currentUserId = _userHelper.GetUserId();
        if (currentUserId != userId && !_userHelper.IsAdminOrModerator())
            return Forbid();

        var cart = await _cartService.RemoveVariantFromCartAsync(userId, variantId, quantity);

        if (cart == null)
            return NotFound(new { message = "Cart or variant not found" });

        return Ok(cart);
    }

    /// <summary>
    /// Vide complètement le panier de l'utilisateur.
    /// Route : DELETE /indeconnect/users/{userId}/cart
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ClearCart([FromRoute] long userId)
    {
        var currentUserId = _userHelper.GetUserId();
        if (currentUserId != userId && !_userHelper.IsAdminOrModerator())
            return Forbid();

        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }

}
