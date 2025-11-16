using System.Security.Claims;
using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/users/{userId}/wishlist")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    private readonly UserHelper _userHelper;
    public WishlistController(IWishlistService wishlistService, UserHelper userHelper)
    {
        _wishlistService = wishlistService;
        _userHelper = userHelper;
    }

    /**
     * Get user's wishlist
     */
    [HttpGet]
    [ProducesResponseType(typeof(WishlistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WishlistDto>> GetWishlist([FromRoute] long userId)
    {
        var currentUserId = _userHelper.GetUserId();
        if (currentUserId != userId && !_userHelper.IsAdminOrModerator())
            return Forbid();

        var wishlist = await _wishlistService.GetUserWishlistAsync(userId);
        return Ok(wishlist);
    }

    /**
     * Add a product to wishlist
     */
    [HttpPost("items")]
    [ProducesResponseType(typeof(WishlistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WishlistDto>> AddToWishlist(
        [FromRoute] long userId,
        [FromBody] AddToWishlistRequest request)
    {
        var currentUserId = _userHelper.GetUserId();
        if (currentUserId != userId)
            return Forbid();

        var wishlist = await _wishlistService.AddProductToWishlistAsync(userId, request.ProductId);
        return Ok(wishlist);
    }

    /**
     * Remove a product from wishlist
     */
    [HttpDelete("items/{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromWishlist(
        [FromRoute] long userId,
        [FromRoute] long productId)
    {
        var currentUserId = _userHelper.GetUserId();
        if (currentUserId != userId)
            return Forbid();

        await _wishlistService.RemoveProductFromWishlistAsync(userId, productId);
        return NoContent();
    }

    /**
     * Check if a product is in user's wishlist
     */
    [HttpGet("items/{productId}/exists")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] 
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> IsInWishlist(
        [FromRoute] long userId,
        [FromRoute] long productId)
    {
        var currentUserId = _userHelper.GetUserId();
        if (currentUserId != userId && !_userHelper.IsAdminOrModerator())
            return Forbid();

        var exists = await _wishlistService.IsProductInWishlistAsync(userId, productId);
        return Ok(new { exists });
    }
}
