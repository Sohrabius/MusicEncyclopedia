using System.Globalization;

namespace MusicEncyclopedia.Web.Middleware;

/// <summary>
/// Middleware that sets the thread culture and HttpContext items (culture,
/// text direction) for the current request.
///
/// LAUNCH MODE: the site currently ships Persian (fa) only. Every request
/// resolves to "fa", and any /en, /ar or /fr URL is redirected (302) to its
/// /fa equivalent so visitors can never reach a non-Persian page. Remove or
/// relax this when multilingual support is re-enabled — the surrounding
/// infrastructure (routes, resources, supported-culture lists) is untouched.
/// </summary>
public class CultureMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Cultures that are still routable but temporarily redirected to Persian.
    /// </summary>
    private static readonly HashSet<string> RedirectCultures = new(
        ["en", "ar", "fr"], StringComparer.OrdinalIgnoreCase);

    public CultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // fa-only launch: fold any other culture-prefixed URL into /fa.
        var path = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(path))
        {
            var firstSegment = path.TrimStart('/').Split('/')[0];
            if (RedirectCultures.Contains(firstSegment))
            {
                var suffix = path.Length > firstSegment.Length + 1
                    ? path.Substring(firstSegment.Length + 1)
                    : "/";
                // 302 (temporary) so browsers don't cache the redirect once
                // multilingual support is switched back on.
                context.Response.Redirect("/fa" + suffix + context.Request.QueryString, permanent: false);
                return;
            }
        }

        // Persian only — the site resolves every request to "fa".
        const string culture = "fa";

        // Set the culture on the current thread
        var cultureInfo = new CultureInfo(culture);
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;

        // Store culture and direction in HttpContext.Items for use in views
        context.Items["culture"] = culture;
        context.Items["dir"] = "rtl";

        // Store in a culture feature for downstream middleware
        context.Features.Set(new CultureFeature(culture, isRtl: true));

        await _next(context);
    }
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
