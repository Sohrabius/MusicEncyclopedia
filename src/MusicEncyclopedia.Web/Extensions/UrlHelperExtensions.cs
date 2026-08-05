using Microsoft.AspNetCore.Mvc;

namespace MusicEncyclopedia.Web.Extensions;

/// <summary>
/// Extension methods for <see cref="IUrlHelper"/> to generate localized URLs.
/// </summary>
public static class UrlHelperExtensions
{
    /// <summary>
    /// Generates a localized URL with the specified culture prefix.
    /// </summary>
    /// <param name="urlHelper">The URL helper.</param>
    /// <param name="culture">Target culture code (e.g., "fa", "en").</param>
    /// <param name="controller">Controller name.</param>
    /// <param name="action">Action name.</param>
    /// <param name="routeValues">Optional route values.</param>
    /// <returns>Localized URL string.</returns>
    public static string LocalizedAction(
        this IUrlHelper urlHelper,
        string culture,
        string action,
        string controller,
        object? routeValues = null)
    {
        var mergedRouteValues = new RouteValueDictionary(routeValues ?? new { })
        {
            ["culture"] = culture
        };

        return urlHelper.Action(action, controller, mergedRouteValues)
               ?? $"/{culture}/{controller}/{action}";
    }

    /// <summary>
    /// Generates a localized URL for a specific entity detail page.
    /// </summary>
    /// <param name="urlHelper">The URL helper.</param>
    /// <param name="culture">Target culture code.</param>
    /// <param name="controller">Controller name (e.g., "Albums", "Tracks").</param>
    /// <param name="slug">Entity slug.</param>
    /// <returns>Localized detail URL.</returns>
    public static string LocalizedEntityUrl(
        this IUrlHelper urlHelper,
        string culture,
        string controller,
        string slug)
    {
        return urlHelper.LocalizedAction(culture, "Detail", controller, new { slug })
               ?? $"/{culture}/{controller}/{slug}";
    }

    /// <summary>
    /// Gets the home URL for the specified culture.
    /// </summary>
    public static string LocalizedHomeUrl(
        this IUrlHelper urlHelper,
        string culture)
    {
        return urlHelper.LocalizedAction(culture, "Index", "Home")
               ?? $"/{culture}";
    }
}
