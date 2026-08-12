using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MusicEncyclopedia.Core.Constants;

namespace MusicEncyclopedia.Web.Filters;

/// <summary>
/// Action filter that validates the culture parameter from route data.
/// Returns a 404 Not Found result if the culture is not supported.
/// </summary>
public class CultureValidationFilter : IActionFilter
{
    private static readonly HashSet<string> SupportedCultures = new(
        CultureConstants.SupportedCultures, StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Try to get culture from route values
        var culture = context.RouteData.Values["culture"] as string;

        // If a culture is present in the route, validate it
        if (!string.IsNullOrWhiteSpace(culture))
        {
            if (!SupportedCultures.Contains(culture))
            {
                context.Result = new NotFoundResult();
                return;
            }
        }

        // If the controller has a [Route] attribute with culture constraint,
        // the routing engine already validated it. This filter is an extra safety net.
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No post-action logic needed
    }
}
