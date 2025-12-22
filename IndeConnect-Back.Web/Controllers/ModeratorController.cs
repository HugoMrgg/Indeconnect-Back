using System.Security.Claims;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/moderator/reviews")]
[Authorize(Roles = "Moderator,Administrator")]
public class ModeratorReviewsController : ControllerBase
{
    private readonly IModerationReviewService _service;

    public ModeratorReviewsController(IModerationReviewService service)
        => _service = service;

    /// <summary>
    /// Liste paginée des reviews (Pending/Approved/Rejected)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status = "Enabled",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null
    )
    {
        ReviewStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(status) &&
            !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse<ReviewStatus>(status, ignoreCase: true, out var s))
                return BadRequest(new { message = $"Status invalide: '{status}'. Valeurs: Enabled, Disabled, All" });

            parsedStatus = s;
        }

        var result = await _service.GetReviewsAsync(parsedStatus, page, pageSize, q);
        return Ok(result);
    }

    /// <summary>
    /// Approve une review (modérateur/admin)
    /// </summary>
    [HttpPost("{reviewId:long}/approve")]
    public async Task<IActionResult> Approve([FromRoute] long reviewId)
    {
        await _service.ApproveAsync(reviewId);
        return NoContent();
    }

    /// <summary>
    /// Reject une review (modérateur/admin)
    /// </summary>
    [HttpPost("{reviewId:long}/reject")]
    public async Task<IActionResult> Reject([FromRoute] long reviewId)
    {
        await _service.RejectAsync(reviewId);
        return NoContent();
    }
}
