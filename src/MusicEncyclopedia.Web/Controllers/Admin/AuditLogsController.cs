using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Admin audit log controller.
/// Route: /admin/audit-log
/// Read-only paged view of the <see cref="AuditLog"/> table written by the audit filter.
/// </summary>
[Route("/admin/audit-log")]
[Authorize(Policy = PermissionConstants.CanManageUsers)]
public sealed class AuditLogsController : AdminBaseController
{
    private const int PageSize = 50;

    private readonly AppDbContext _db;

    public AuditLogsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        int page = 1,
        string? q = null,
        string? entityType = null,
        bool? success = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(a =>
                a.UserName.Contains(term) ||
                a.Action.Contains(term) ||
                (a.Details != null && a.Details.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType != null && a.EntityType == entityType);
        }

        if (success.HasValue)
        {
            query = query.Where(a => a.IsSuccess == success.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = CalculateTotalPages(totalItems, PageSize);
        page = NormalizePage(page, totalPages);

        var rows = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(a => new AuditLogListItem
            {
                AuditLogId = a.AuditLogId,
                Timestamp = a.Timestamp,
                UserName = a.UserName,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                IsSuccess = a.IsSuccess,
                Details = a.Details
            })
            .ToListAsync(cancellationToken);

        ViewData["Title"] = "Audit Log";
        ViewData["ActiveMenu"] = "Audit Log";

        return View(new AuditLogListViewModel
        {
            Items = PagedResult<AuditLogListItem>.Create(rows, page, PageSize, totalItems),
            SearchQuery = q,
            EntityTypeFilter = entityType,
            SuccessFilter = success,
            Page = page,
            PageSize = PageSize
        });
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var entry = await _db.AuditLogs.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AuditLogId == id, cancellationToken);

        if (entry is null)
        {
            SetErrorMessage("Audit log entry not found.");
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = $"Audit Entry #{entry.AuditLogId}";
        ViewData["ActiveMenu"] = "Audit Log";

        return View(new AuditLogDetailViewModel
        {
            AuditLogId = entry.AuditLogId,
            Timestamp = entry.Timestamp,
            UserName = entry.UserName,
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            IsSuccess = entry.IsSuccess,
            Details = entry.Details,
            IpAddress = entry.IpAddress
        });
    }
}
