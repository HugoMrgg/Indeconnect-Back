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
    private readonly UserHelper _userHelper;
    public BrandSubscriptionController(IBrandSubscriptionService subscriptionService, UserHelper userHelper)
    {
        _subscriptionService = subscriptionService;
        _userHelper = userHelper;
    }
    
    /**
     * Subscribe to a brand
     * <param name="request">Brand subscription request</param>
     * <returns>Created subscription details</returns>
     */
    [HttpPost]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(BrandSubscriptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BrandSubscriptionResponse>> SubscribeToBrand(
        [FromBody] CreateBrandSubscriptionRequest request)
    {
        try
        {
            var userId = _userHelper.GetUserId();
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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /**
     * Get all brand subscriptions for the authenticated user
     * <returns>List of subscriptions</returns>
     */
    [HttpGet]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(UserBrandSubscriptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserBrandSubscriptionsResponse>> GetSubscriptions()
    {
        try
        {
            var userId = _userHelper.GetUserId();
            var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId);
            return Ok(subscriptions);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /**
     * Unsubscribe from a brand
     * <param name="brandId">Brand ID to unsubscribe from</param>
     * <returns>No content on success</returns>
     */
    [HttpDelete("{brandId}")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnsubscribeFromBrand([FromRoute] long brandId)
    {
        try
        {
            var userId = _userHelper.GetUserId();
            await _subscriptionService.UnsubscribeFromBrandAsync(userId, brandId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
