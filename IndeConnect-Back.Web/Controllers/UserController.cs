using IndeConnect_Back.Application.DTOs.Users;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IndeConnect_Back.Domain.user;
using IndeConnect_Back.Web;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/users")]
[Authorize]
public class UserController : ControllerBase
{

    private readonly IUserService _userService;
    private readonly UserHelper _userHelper;

    public UserController(IUserService userService, UserHelper userHelper)
    {
        _userService = userService;
        _userHelper  = userHelper;
    }


    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{userId}")]
    [Authorize(Policy = "RequireUserIdMatch")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailDto>> GetUserById([FromRoute] long userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(user);
    }

    /// <summary>
    /// Liste des comptes administratifs selon les droits de l’utilisateur connecté
    /// </summary>
    [HttpGet("accounts")]
    [Authorize(Roles = "Administrator,Moderator,SuperVendor")]
    [ProducesResponseType(typeof(List<AccountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AccountDto>>> GetAllAccounts()
    {
        var currentUserId = _userHelper.GetUserId();

        var roleClaim = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim))
            return Forbid();

        if (!Enum.TryParse<Role>(roleClaim, out var currentUserRole))
            return Forbid();

        var accounts = await _userService.GetAllAccountsAsync(currentUserId, currentUserRole);
        return Ok(accounts);
    }


    /// <summary>
    /// Active ou désactive un compte
    /// </summary>
    [HttpPatch("{accountId}/toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleAccountStatus(
        [FromRoute] long accountId,
        [FromBody] ToggleAccountStatusRequest request)
    {
        try
        {
            await _userService.ToggleAccountStatusAsync(accountId, request.IsEnabled);
            return Ok(new { message = "Account status updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Request pour toggle le statut d'un compte
    /// </summary>
    public record ToggleAccountStatusRequest(bool IsEnabled);
}