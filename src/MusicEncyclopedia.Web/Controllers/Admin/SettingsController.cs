using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Admin settings controller.
/// Route: /admin/settings
/// Read-only overview of the application configuration and runtime environment,
/// useful for ops diagnostics (which provider, cultures, cache settings, etc.).
/// </summary>
[Route("/admin/settings")]
[Authorize(Policy = PermissionConstants.CanManageUsers)]
public sealed class SettingsController : AdminBaseController
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _environment;
    private readonly IOptions<IdentityOptions> _identityOptions;

    public SettingsController(
        IConfiguration config,
        IWebHostEnvironment environment,
        IOptions<IdentityOptions> identityOptions)
    {
        _config = config;
        _environment = environment;
        _identityOptions = identityOptions;
    }

    [HttpGet]
    [Route("")]
    [Route("Index")]
    public IActionResult Index()
    {
        var dbProvider = _config.GetValue<string>("DatabaseProvider") ?? "SqlServer";

        var viewModel = new SettingsViewModel
        {
            Environment = _environment.EnvironmentName,
            RuntimeVersion = Environment.Version.ToString(),
            DatabaseProvider = dbProvider,
            DefaultCulture = CultureConstants.DefaultCulture,
            BaseCulture = CultureConstants.BaseCulture,
            FallbackCulture = CultureConstants.FallbackCulture,
            SupportedCultures = CultureConstants.SupportedCultures,
            SiteBaseUrl = _config["Site:BaseUrl"],
            DefaultPublicPageCacheMinutes = _config.GetValue<int>("Cache:DefaultPublicPageMinutes"),
            DefaultPageSize = _config.GetValue<int>("Search:PageSize"),
            MaxLoginAttempts = _identityOptions.Value.Lockout.MaxFailedAccessAttempts,
            MediaStoragePath = _config["Media:StoragePath"] ?? "media",
            SearchProvider = _config["Search:Provider"] ?? "None"
        };

        ViewData["Title"] = "Settings";
        ViewData["ActiveMenu"] = "Settings";

        return View(viewModel);
    }
}
