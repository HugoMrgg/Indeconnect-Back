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

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    /// <summary>
    /// Get user's wishlist
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<WishlistDto>> GetWishlist([FromRoute] long userId)
    {
        try
        {
            // Vérifier que l'utilisateur accède à sa propre wishlist
            var currentUserId = GetCurrentUserId();
            if (currentUserId != userId && !IsAdminOrModerator())
                return Forbid();

            var wishlist = await _wishlistService.GetUserWishlistAsync(userId);
            return Ok(wishlist);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Add a product to wishlist
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<WishlistDto>> AddToWishlist(
        [FromRoute] long userId,
        [FromBody] AddToWishlistRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != userId)
                return Forbid();

            var wishlist = await _wishlistService.AddProductToWishlistAsync(userId, request.ProductId);
            return Ok(wishlist);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a product from wishlist
    /// </summary>
    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(
        [FromRoute] long userId,
        [FromRoute] long productId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != userId)
                return Forbid();

            await _wishlistService.RemoveProductFromWishlistAsync(userId, productId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Check if a product is in user's wishlist
    /// </summary>
    [HttpGet("items/{productId}/exists")]
    public async Task<ActionResult<bool>> IsInWishlist(
        [FromRoute] long userId,
        [FromRoute] long productId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId != userId && !IsAdminOrModerator())
            return Forbid();

        var exists = await _wishlistService.IsProductInWishlistAsync(userId, productId);
        return Ok(new { exists });
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token");

        return userId;
    }

    private bool IsAdminOrModerator()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return role == "Administrator" || role == "Moderator";
    }
}
