using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Services.Services;

namespace MusicEncyclopedia.Web.Filters;

/// <summary>
/// Records admin-area write operations (any non-GET request handled by a controller
/// in the Admin area) to the <see cref="AuditLog"/> table. Read-only GET requests,
/// requests that threw an exception, and the audit log itself are skipped to avoid noise.
/// </summary>
public sealed class AuditLogFilter : IAsyncActionFilter
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuditLogFilter> _logger;
    private readonly CacheInvalidationService _cacheInvalidation;

    public AuditLogFilter(
        AppDbContext db,
        ILogger<AuditLogFilter> logger,
        CacheInvalidationService cacheInvalidation)
    {
        _db = db;
        _logger = logger;
        _cacheInvalidation = cacheInvalidation;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        // Only audit requests that were authorized and actually executed.
        if (executed.Exception is not null)
        {
            return;
        }

        var http = executed.HttpContext;
        if (HttpMethods.IsGet(http.Request.Method))
        {
            return;
        }

        var route = executed.RouteData.Values;
        if (route["area"] as string != "Admin")
        {
            return;
        }

        var controller = route["controller"] as string ?? "";
        var action = route["action"] as string ?? "";
        if (string.Equals(controller, "AuditLog", StringComparison.OrdinalIgnoreCase))
        {
            return; // avoid auditing the audit log
        }

        // Map the admin controller name to the entity type code it manages. The
        // naive TrimEnd('s') heuristic mangles plurals (Media → Medi, Companies → Companie),
        // so an explicit map is used with a safe fallback.
        var entityType = EntityTypeByController.GetValueOrDefault(controller, controller.TrimEnd('s'));
        int? entityId = null;
        if (route.TryGetValue("id", out var idValue) && int.TryParse(idValue?.ToString(), out var parsedId))
        {
            entityId = parsedId;
        }
        else if (int.TryParse(http.Request.Form["entityId"], out var formEntityId))
        {
            entityId = formEntityId;
        }

        var isSuccess = executed.Result switch
        {
            RedirectToActionResult => true,
            RedirectToRouteResult => true,
            LocalRedirectResult => true,
            JsonResult { Value: { } value } when value is not null => ResolveJsonSuccess(value),
            _ => false
        };

        var details = isSuccess || executed.Result is null ? null : DescribeResult(executed.Result);

        try
        {
            _db.AuditLogs.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserName = http.User.Identity?.Name ?? "anonymous",
                Action = $"{controller}.{action}",
                EntityType = string.IsNullOrEmpty(entityType) ? null : entityType,
                EntityId = entityId,
                IsSuccess = isSuccess,
                Details = details,
                IpAddress = http.Connection.RemoteIpAddress?.ToString()
            });
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Auditing must never break the request.
            _logger.LogWarning(ex, "Failed to write audit log entry for {Action}", $"{controller}.{action}");
        }

        // Phase 5 (§15.1): invalidate cached public pages after any successful admin
        // write so the site reflects the change immediately. Runs after the DB write
        // and is best-effort — invalidation failures must never break the request.
        if (isSuccess)
        {
            await InvalidateCacheAsync(controller, entityType, entityId, action);
        }
    }

    private async Task InvalidateCacheAsync(
        string controller,
        string entityType,
        int? entityId,
        string action)
    {
        try
        {
            // Cross-cutting operations affect too many pages to invalidate individually.
            if (entityType is "LookupTable" or "Dashboard" or "User" or "Role" or "Settings")
            {
                await _cacheInvalidation.InvalidateBroadAsync();
                return;
            }

            if (string.IsNullOrWhiteSpace(entityType) || entityType is "Unknown" or "AuditLog")
                return;

            await _cacheInvalidation.InvalidateEntityAsync(
                entityType, entityId ?? 0, DeriveChangeType(action));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache invalidation failed for {Action}", $"{controller}.{action}");
        }
    }

    private static string DeriveChangeType(string action)
    {
        if (action.Contains("Delete", StringComparison.OrdinalIgnoreCase))
            return "Deleted";
        if (action.Contains("Restore", StringComparison.OrdinalIgnoreCase))
            return "Restored";
        if (action.Contains("Create", StringComparison.OrdinalIgnoreCase)
            || action.Contains("Add", StringComparison.OrdinalIgnoreCase))
            return "Created";
        return "Updated";
    }

    /// <summary>Maps admin controller names to entity type codes for the audit trail.</summary>
    private static readonly Dictionary<string, string> EntityTypeByController = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Albums"] = "Album",
        ["Tracks"] = "Track",
        ["People"] = "Person",
        ["Companies"] = "Company",
        ["Poems"] = "Poem",
        ["Genres"] = "Genre",
        ["Moods"] = "Mood",
        ["Instruments"] = "Instrument",
        ["SungVersions"] = "SungVersion",
        ["Publications"] = "Publication",
        ["Sessions"] = "RecordingSession",
        ["Events"] = "PerformanceEvent",
        ["Locations"] = "Location",
        ["Awards"] = "Award",
        ["Certifications"] = "Certification",
        ["Charts"] = "Chart",
        ["Media"] = "Media",
        ["Sources"] = "Source",
        ["Citations"] = "Citation",
        ["Tags"] = "Tag",
        ["Attributes"] = "AttributeValue",
        ["Localizations"] = "Localization",
        ["RelatedItems"] = "RelatedItem",
        ["AlbumTracklist"] = "AlbumTrack",
        ["LookupTables"] = "LookupTable",
        ["Users"] = "User",
        ["Roles"] = "Role",
        ["Dashboard"] = "Dashboard"
    };

    private static bool ResolveJsonSuccess(object value)
    {
        if (value is System.Text.Json.JsonElement element &&
            element.TryGetProperty("success", out var successProp) &&
            successProp.ValueKind == System.Text.Json.JsonValueKind.True)
        {
            return true;
        }

        // Newtonsoft-style anonymous objects
        var prop = value.GetType().GetProperty("success");
        return prop?.GetValue(value) is true;
    }

    private static string? DescribeResult(IActionResult result)
    {
        if (result is JsonResult { Value: { } json } && json is not null)
        {
            var errors = json.GetType().GetProperty("errors")?.GetValue(json);
            return errors?.ToString() is { Length: > 0 } text
                ? $"Validation failed: {text}"
                : "Request failed";
        }

        if (result is ViewResult)
        {
            return "Form re-displayed (validation failed)";
        }

        return null;
    }
}
