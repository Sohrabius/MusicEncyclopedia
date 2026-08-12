using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>View model for the audit log list page.</summary>
public sealed class AuditLogListViewModel
{
    public PagedResult<AuditLogListItem> Items { get; init; } = PagedResult<AuditLogListItem>.Create([], 1, 50, 0);
    public string? SearchQuery { get; init; }
    public string? EntityTypeFilter { get; init; }
    public bool? SuccessFilter { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>A row in the audit log list.</summary>
public sealed class AuditLogListItem
{
    public int AuditLogId { get; init; }
    public DateTime Timestamp { get; init; }
    public string UserName { get; init; } = "";
    public string Action { get; init; } = "";
    public string? EntityType { get; init; }
    public int? EntityId { get; init; }
    public bool IsSuccess { get; init; }
    public string? Details { get; init; }
}

/// <summary>View model for a single audit log entry.</summary>
public sealed class AuditLogDetailViewModel
{
    public int AuditLogId { get; init; }
    public DateTime Timestamp { get; init; }
    public string UserName { get; init; } = "";
    public string Action { get; init; } = "";
    public string? EntityType { get; init; }
    public int? EntityId { get; init; }
    public bool IsSuccess { get; init; }
    public string? Details { get; init; }
    public string? IpAddress { get; init; }
}
