using System.Globalization;

namespace MusicEncyclopedia.Web.Middleware;

/// <summary>
/// Middleware that reads the culture from the first route segment,
/// validates it against supported cultures, and sets the thread culture
/// along with HttpContext items for culture and text direction.
/// </summary>
public class CultureMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> SupportedCultures = new(
        StringComparer.OrdinalIgnoreCase) { "fa" };

    private const string DefaultCulture = "fa";

    public CultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var culture = ResolveCulture(context);

        // Set the culture on the current thread
        var cultureInfo = new CultureInfo(culture);
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;

        // Store culture and direction in HttpContext.Items for use in views
        context.Items["culture"] = culture;
        context.Items["dir"] = IsRtl(culture) ? "rtl" : "ltr";

        // Store in a culture feature for downstream middleware
        context.Features.Set(new CultureFeature(culture, IsRtl(culture)));

        await _next(context);
    }

    /// <summary>
    /// Resolves the culture from the route data, falling back to default.
    /// </summary>
    private static string ResolveCulture(HttpContext context)
    {
        // First, try from route data (set by the routing middleware)
        var routeCulture = context.Request.RouteValues["culture"] as string;

        if (!string.IsNullOrWhiteSpace(routeCulture) && SupportedCultures.Contains(routeCulture))
        {
            return routeCulture.ToLowerInvariant();
        }

        // Second, try from query string
        var queryCulture = context.Request.Query["culture"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(queryCulture) && SupportedCultures.Contains(queryCulture))
        {
            return queryCulture.ToLowerInvariant();
        }

        // Third, try from cookie
        var cookieCulture = context.Request.Cookies["culture"];
        if (!string.IsNullOrWhiteSpace(cookieCulture) && SupportedCultures.Contains(cookieCulture))
        {
            return cookieCulture.ToLowerInvariant();
        }

        // Fourth, try from Accept-Language header
        try
        {
            var acceptLanguage = context.Request.GetTypedHeaders().AcceptLanguage;
            if (acceptLanguage is not null && acceptLanguage.Count > 0)
            {
                foreach (var lang in acceptLanguage)
                {
                    if (lang.Value.HasValue)
                    {
                        var langValue = lang.Value.Value;
                        if (!string.IsNullOrWhiteSpace(langValue))
                        {
                            var culturePart = langValue.Split('-')[0].ToLowerInvariant();
                            if (SupportedCultures.Contains(culturePart))
                            {
                                return culturePart;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        // Fall back to default
        return DefaultCulture;
    }

    /// <summary>
    /// Determines if the given culture is right-to-left.
    /// </summary>
    private static bool IsRtl(string culture) => culture == "fa";
}

/// <summary>
/// Feature class to carry culture information through the middleware pipeline.
/// </summary>
public sealed class CultureFeature
{
    public string Culture { get; }
    public bool IsRtl { get; }

    public CultureFeature(string culture, bool isRtl)
    {
        Culture = culture;
        IsRtl = isRtl;
    }
}
