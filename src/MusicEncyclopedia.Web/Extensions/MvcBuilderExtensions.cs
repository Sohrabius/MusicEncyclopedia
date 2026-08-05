using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to configure
/// culture-aware MVC with localization support.
/// </summary>
public static class MvcBuilderExtensions
{
    /// <summary>
    /// Adds culture-aware MVC services with controller and view support,
    /// localization, and the standard route patterns.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="supportedCultures">Array of supported culture codes (e.g., "fa", "en", "ar", "fr").</param>
    /// <returns>The <see cref="IMvcBuilder"/> for further chaining.</returns>
    public static IMvcBuilder AddCultureAwareMvc(
        this IServiceCollection services,
        string[]? supportedCultures = null)
    {
        supportedCultures ??= ["fa"];

        // Configure localization services
        services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        // Configure request localization options
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var cultures = supportedCultures
                .Select(c => new CultureInfo(c))
                .ToArray();

            options.DefaultRequestCulture = new RequestCulture(supportedCultures[0]);
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;

            // Clear default providers; culture is resolved via route in CultureMiddleware
            options.RequestCultureProviders.Clear();
        });

        // Add controllers with views and localization
        var mvcBuilder = services
            .AddControllersWithViews(options =>
            {
                // Apply culture validation filter globally
                options.Filters.Add<MusicEncyclopedia.Web.Filters.CultureValidationFilter>();
            })
            .AddViewLocalization()
            .AddDataAnnotationsLocalization();

        return mvcBuilder;
    }

    /// <summary>
    /// Configures conventional routes with culture-aware patterns.
    /// Call this on the <see cref="WebApplication"/> after UseRouting.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseCultureAwareRoutes(this WebApplication app)
    {
        // Default localized route
        app.MapControllerRoute(
            name: "localized-default",
            pattern: "{culture:regex(^(fa)$)}/{controller=Home}/{action=Index}/{id?}");

        // Fallback route (no culture prefix — redirects or uses default)
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Area route for admin
        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

        return app;
    }
}
