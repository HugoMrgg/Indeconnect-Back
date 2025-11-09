using System.Security.Claims;
using IndeConnect_Back.Web.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndeConnect_Back.Web.Handlers;

public class GetuserIdHandler
    : AuthorizationHandler<UserIdAttribute>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserIdAttribute requirement)
    {
        // Extract the HttpContext from the resource (supports controller and filter contexts)
        var httpContext = context.Resource switch
        {
            HttpContext ctx => ctx,
            AuthorizationFilterContext mvcCtx => mvcCtx.HttpContext,
            _ => null
        };

        if (httpContext == null)
            return Task.CompletedTask;

        // Get the authenticated user ID from the token claims
        var authenticatedUserIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(authenticatedUserIdClaim, out var authenticatedUserId))
            return Task.CompletedTask;

        // Get the user's role from the claims
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value?.ToLowerInvariant();

        // If the user's role is administrator, authorize immediately
        if (userRole == "administrator")
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Get the requested user ID from the route
        var routeData = httpContext.GetRouteData();
        if (routeData.Values.TryGetValue("userId", out var userIdObj) && 
            long.TryParse(userIdObj?.ToString(), out var requestedUserId))
        {
            // Allow if user is requesting their own information
            if (authenticatedUserId == requestedUserId)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}