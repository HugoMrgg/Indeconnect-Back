// src/IndeConnect-Back.Web/Helpers/UserHelper.cs
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

    /// <summary>
    /// Récupère l'ID de l'utilisateur connecté (nullable pour les endpoints publics)
    /// </summary>
    public long? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }

    /// <summary>
    /// Récupère l'ID de l'utilisateur connecté (throw si non connecté - pour endpoints protégés)
    /// </summary>
    public long GetUserIdOrThrow()
    {
        var userId = GetUserId();
        
        if (!userId.HasValue)
            throw new UnauthorizedAccessException("User not authenticated");

        return userId.Value;
    }

    public bool IsAdminOrModerator()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var role = user?.FindFirst(ClaimTypes.Role)?.Value;

        return role is "Administrator" or "Moderator";
    }
}