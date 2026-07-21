using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JiraTrack.Settings;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRolesAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    public AuthorizeRolesAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (string.IsNullOrEmpty(Roles)) return;

        var allowedRoles = Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (!allowedRoles.Any(role => user.IsInRole(role)))
            context.Result = new ForbidResult();
    }
}

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string ProjectManager = "ProjectManager";
    public const string Developer = "Developer";
    public const string QA = "QA";
    public const string Viewer = "Viewer";

    public static readonly string[] All = [Admin, ProjectManager, Developer, QA, Viewer];
}
