using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace IndeConnect_Back.Web;

public class UserHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token");

        return userId;
    }

    public bool IsAdminOrModerator()
    {
        var user  = _httpContextAccessor.HttpContext?.User;
        var role  = user?.FindFirst(ClaimTypes.Role)?.Value;

        return role is "Administrator" or "Moderator";
    }
}