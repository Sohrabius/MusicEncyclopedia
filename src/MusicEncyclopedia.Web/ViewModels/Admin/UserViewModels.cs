using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>View model for the admin user list page.</summary>
public sealed class UserListViewModel
{
    public PagedResult<UserListItem> Items { get; init; } = PagedResult<UserListItem>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>A row in the admin user list.</summary>
public sealed class UserListItem
{
    public string Id { get; init; } = "";
    public string Email { get; init; } = "";
    public IReadOnlyList<string> Roles { get; init; } = [];
    public int PermissionCount { get; init; }
    public bool IsLockedOut { get; init; }
    public bool EmailConfirmed { get; init; }
}

/// <summary>View model for the user edit form.</summary>
public sealed class UserEditViewModel
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public bool EmailConfirmed { get; set; }
    public bool IsLockedOut { get; set; }

    public IReadOnlyList<string> AllRoles { get; set; } = [];
    public IReadOnlyList<string> SelectedRoles { get; set; } = [];

    public IReadOnlyList<string> AllPermissions { get; set; } = [];
    public IReadOnlyList<string> SelectedPermissions { get; set; } = [];
}

/// <summary>View model for the admin role list page.</summary>
public sealed class RoleListViewModel
{
    public IReadOnlyList<RoleListItem> Items { get; init; } = [];
}

/// <summary>A row in the admin role list.</summary>
public sealed class RoleListItem
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public int UserCount { get; init; }
    public int PermissionCount { get; init; }
}

/// <summary>View model for the role create/edit form.</summary>
public sealed class RoleEditViewModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public IReadOnlyList<string> AllPermissions { get; set; } = [];
    public IReadOnlyList<string> SelectedPermissions { get; set; } = [];
}
