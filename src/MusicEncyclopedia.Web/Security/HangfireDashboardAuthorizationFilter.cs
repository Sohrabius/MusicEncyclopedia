using Hangfire.Dashboard;
using MusicEncyclopedia.Core.Constants;

namespace MusicEncyclopedia.Web.Security;

/// <summary>
/// Restricts the Hangfire dashboard (spec §20) to authenticated administrators —
/// any user holding at least one application permission claim.
/// </summary>
public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && PermissionConstants.All.Any(p => httpContext.User.HasClaim("Permission", p));
    }
}
