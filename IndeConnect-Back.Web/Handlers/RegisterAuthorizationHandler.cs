using System.Security.Claims;
using System.Text.Json;
using IndeConnect_Back.Web.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace IndeConnect_Back.Web.Handlers;

public class RegisterAuthorizationHandler
    : AuthorizationHandler<RoleAuthorizationAttribute>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleAuthorizationAttribute requirement)
    {
        // Extract the HttpContext from the resource (supports controller/filter contexts)
        var httpContext = context.Resource switch
        {
            HttpContext ctx => ctx,
            Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext mvcCtx => mvcCtx.HttpContext,
            _ => null
        };
        if (httpContext == null)
            return;

        // Read and buffer the request body to extract targetRole
        httpContext.Request.EnableBuffering();
        string body;
        using (var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            httpContext.Request.Body.Position = 0;
        }

        // Parse the JSON body to retrieve the 'targetRole' field
        string? targetRole = null;
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                var json = JsonDocument.Parse(body);
                if (json.RootElement.TryGetProperty("targetRole", out var roleProp))
                    targetRole = roleProp.GetString();
            }
            catch { /* ignore parsing errors */ }
        }

        // Find the caller's role from claims, or fallback to 'anonymous' if not authenticated
        var userRole = GetCurrentRole(context.User);

        // Core access rule: only permit if current role is allowed to register for targetRole
        if (CanCreate(userRole, targetRole))
            context.Succeed(requirement);
    }

    private string GetCurrentRole(ClaimsPrincipal user)
    {
        if (!user.Identity?.IsAuthenticated ?? true)
            return "anonymous";

        return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "anonymous";
    }

    private bool CanCreate(string currentRole, string? targetRole)
    {
        if (string.IsNullOrWhiteSpace(targetRole)) return false;
        currentRole = currentRole.ToLowerInvariant();
        targetRole = targetRole.ToLowerInvariant();

        return currentRole switch
        {
            "anonymous" => targetRole == "client",
            "vendor" => false,
            "supervendor" => targetRole == "vendor",
            "moderator" => targetRole is "vendor" or "supervendor",
            "administrator" => targetRole == "moderator",
            _ => false
        };
    }
}