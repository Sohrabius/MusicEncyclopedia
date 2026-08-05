using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Services.Services;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Abstract base controller for all admin-area controllers (spec 10.1, 10.2).
/// All admin pages require the "AdminArea" policy, which grants access to any
/// authenticated user holding at least one admin permission claim.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "AdminArea")]
public abstract class AdminBaseController : Controller
{
    /// <summary>
    /// Stores a success message in TempData that persists to the next request.
    /// Accessed in the admin layout to render a green alert banner.
    /// </summary>
    protected void SetSuccessMessage(string message)
    {
        TempData["SuccessMessage"] = message;
    }

    /// <summary>
    /// Stores an error message in TempData that persists to the next request.
    /// Accessed in the admin layout to render a red alert banner.
    /// </summary>
    protected void SetErrorMessage(string message)
    {
        TempData["ErrorMessage"] = message;
    }

    /// <summary>
    /// Stores an info message in TempData.
    /// </summary>
    protected void SetInfoMessage(string message)
    {
        TempData["InfoMessage"] = message;
    }

    /// <summary>
    /// Stores a warning message in TempData for the admin layout banner.
    /// </summary>
    protected void SetWarningMessage(string message)
    {
        TempData["WarningMessage"] = message;
    }

    /// <summary>
    /// Gets the <see cref="CacheInvalidationService"/> from the request services.
    /// </summary>
    protected CacheInvalidationService CacheInvalidationService =>
        _cacheInvalidationService ??= HttpContext.RequestServices.GetRequiredService<CacheInvalidationService>();
    private CacheInvalidationService? _cacheInvalidationService;

    /// <summary>
    /// Invalidates cached data for a changed entity.
    /// </summary>
    /// <param name="entityTypeCode">The entity type code (e.g., "Album", "Track").</param>
    /// <param name="entityId">The ID of the entity that changed.</param>
    /// <param name="changeType">The change type: "Created", "Updated", "Deleted", "Restored".</param>
    protected async Task InvalidateEntityCacheAsync(string entityTypeCode, int entityId, string changeType)
    {
        await CacheInvalidationService.InvalidateEntityAsync(entityTypeCode, entityId, changeType);
    }

    /// <summary>
    /// Invalidates cached data for a changed entity identified by EntityTypeId.
    /// </summary>
    /// <param name="entityTypeId">The numeric entity type ID from the EntityType table.</param>
    /// <param name="entityId">The ID of the entity that changed.</param>
    /// <param name="changeType">The change type.</param>
    protected async Task InvalidateEntityCacheAsync(int entityTypeId, int entityId, string changeType)
    {
        await CacheInvalidationService.InvalidateEntityAsync(entityTypeId, entityId, changeType);
    }

    /// <summary>
    /// Invalidates broad caches (home page, search results).
    /// </summary>
    protected async Task InvalidateBroadCacheAsync()
    {
        await CacheInvalidationService.InvalidateBroadAsync();
    }

    /// <summary>
    /// Calculates the total number of pages given total item count and page size.
    /// </summary>
    protected static int CalculateTotalPages(int totalItems, int pageSize)
    {
        return totalItems > 0
            ? (int)Math.Ceiling(totalItems / (double)pageSize)
            : 0;
    }

    /// <summary>
    /// Validates and normalizes the page number to be within valid bounds.
    /// </summary>
    protected static int NormalizePage(int page, int totalPages)
    {
        if (page < 1) return 1;
        if (totalPages > 0 && page > totalPages) return totalPages;
        return page;
    }

    /// <summary>
    /// Redirects back to the previous page (HTTP Referer) or falls back to the
    /// admin dashboard if no referer is available.
    /// </summary>
    protected IActionResult RedirectBack(string fallbackUrl = "/admin")
    {
        var referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrWhiteSpace(referer))
        {
            return Redirect(referer);
        }
        return Redirect(fallbackUrl);
    }
}
