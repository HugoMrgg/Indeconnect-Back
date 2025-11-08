using System.Security.Claims;
using IndeConnect_Back.Application.DTOs.Subscriptions;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/brandSubscriptions")]
[Authorize]
public class BrandSubscriptionController : ControllerBase
{
    private readonly IBrandSubscriptionService _subscriptionService;

    public BrandSubscriptionController(IBrandSubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Subscribe to a brand
    /// POST /indeconnect/users/{userId}/brand-subscriptions
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BrandSubscriptionResponse>> SubscribeToBrand(
        [FromRoute] long userId, 
        [FromBody] CreateBrandSubscriptionRequest request)
    {
        try
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            if (authenticatedUserId != userId)
                return Forbid();

            var response = await _subscriptionService.SubscribeToBrandAsync(userId, request.BrandId);
            return CreatedAtAction(
                nameof(GetSubscriptions), 
                new { userId }, 
                response
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all brand subscriptions for a user
    /// GET /indeconnect/users/{userId}/brand-subscriptions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UserBrandSubscriptionsResponse>> GetSubscriptions(
        [FromRoute] long userId)
    {
        try
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            if (authenticatedUserId != userId)
                return Forbid();

            var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId);
            return Ok(subscriptions);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Unsubscribe from a brand
    /// DELETE /indeconnect/users/{userId}/brand-subscriptions/{brandId}
    /// </summary>
    [HttpDelete("{brandId}")]
    public async Task<IActionResult> UnsubscribeFromBrand(
        [FromRoute] long userId, 
        [FromRoute] long brandId)
    {
        try
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            if (authenticatedUserId != userId)
                return Forbid();

            await _subscriptionService.UnsubscribeFromBrandAsync(userId, brandId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // tODO : à faire un user service
    private long GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token");
        
        return userId;
    }
}
