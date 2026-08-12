using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Admin role management controller.
/// Route: /admin/roles
/// Manages role definitions and the permission claims attached to each role.
/// The <see cref="Security.AppUserClaimsPrincipalFactory"/> surfaces role permission
/// claims onto the signed-in principal so permission policies (RequireClaim("Permission", ...))
/// are satisfied for users in that role.
/// </summary>
[Route("/admin/roles")]
[Authorize(Policy = PermissionConstants.CanManageUsers)]
public sealed class RolesController : AdminBaseController
{
    private const string PermissionClaimType = "Permission";

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<RolesController> _logger;

    public RolesController(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        ILogger<RolesController> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var items = new List<RoleListItem>();
        foreach (var role in _roleManager.Roles.OrderBy(r => r.Name))
        {
            var permissions = (await _roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToList();

            items.Add(new RoleListItem
            {
                Id = role.Id,
                Name = role.Name ?? "",
                PermissionCount = permissions.Count,
                UserCount = (await _userManager.GetUsersInRoleAsync(role.Name ?? "")).Count
            });
        }

        ViewData["Title"] = "Roles";
        ViewData["ActiveMenu"] = "Roles";

        return View(new RoleListViewModel { Items = items });
    }

    [HttpGet]
    [Route("Create")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Role";
        ViewData["ActiveMenu"] = "Roles";

        return View("Edit", new RoleEditViewModel
        {
            AllPermissions = PermissionConstants.All
        });
    }

    [HttpGet]
    [Route("{id}/Edit")]
    public async Task<IActionResult> Edit(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            SetErrorMessage("Role not found.");
            return RedirectToAction(nameof(Index));
        }

        var permissions = (await _roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == PermissionClaimType)
            .Select(c => c.Value)
            .ToList();

        ViewData["Title"] = $"Edit Role: {role.Name}";
        ViewData["ActiveMenu"] = "Roles";

        return View("Edit", new RoleEditViewModel
        {
            Id = role.Id,
            Name = role.Name ?? "",
            AllPermissions = PermissionConstants.All,
            SelectedPermissions = permissions
        });
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleEditViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Role";
            ViewData["ActiveMenu"] = "Roles";
            viewModel.AllPermissions = PermissionConstants.All;
            return View("Edit", viewModel);
        }

        var roleName = viewModel.Name.Trim();
        var existing = await _roleManager.FindByNameAsync(roleName);
        if (existing is not null)
        {
            ModelState.AddModelError(nameof(viewModel.Name), "A role with this name already exists.");
            ViewData["Title"] = "Create Role";
            ViewData["ActiveMenu"] = "Roles";
            viewModel.AllPermissions = PermissionConstants.All;
            return View("Edit", viewModel);
        }

        var role = new IdentityRole(roleName);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            ViewData["Title"] = "Create Role";
            ViewData["ActiveMenu"] = "Roles";
            viewModel.AllPermissions = PermissionConstants.All;
            return View("Edit", viewModel);
        }

        await ReplacePermissionsAsync(role, viewModel.SelectedPermissions ?? []);

        _logger.LogInformation("Role {Role} created by {Admin} with {Count} permissions",
            roleName, User.Identity?.Name, (viewModel.SelectedPermissions ?? []).Count);

        SetSuccessMessage($"Role \"{roleName}\" created successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, RoleEditViewModel viewModel)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            SetErrorMessage("Role not found.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = $"Edit Role: {role.Name}";
            ViewData["ActiveMenu"] = "Roles";
            viewModel.AllPermissions = PermissionConstants.All;
            return View("Edit", viewModel);
        }

        if (role.Name != viewModel.Name.Trim())
        {
            var duplicate = await _roleManager.FindByNameAsync(viewModel.Name.Trim());
            if (duplicate is not null && duplicate.Id != role.Id)
            {
                ModelState.AddModelError(nameof(viewModel.Name), "A role with this name already exists.");
                ViewData["Title"] = $"Edit Role: {role.Name}";
                ViewData["ActiveMenu"] = "Roles";
                viewModel.AllPermissions = PermissionConstants.All;
                return View("Edit", viewModel);
            }

            var renameResult = await _roleManager.SetRoleNameAsync(role, viewModel.Name.Trim());
            if (!renameResult.Succeeded)
            {
                AddIdentityErrors(renameResult);
                ViewData["Title"] = $"Edit Role: {role.Name}";
                ViewData["ActiveMenu"] = "Roles";
                viewModel.AllPermissions = PermissionConstants.All;
                return View("Edit", viewModel);
            }
            await _roleManager.UpdateAsync(role);
        }

        await ReplacePermissionsAsync(role, viewModel.SelectedPermissions ?? []);

        _logger.LogInformation("Role {Role} updated by {Admin}",
            role.Name, User.Identity?.Name);

        SetSuccessMessage($"Role \"{role.Name}\" updated successfully.");
        return RedirectToAction(nameof(Edit), new { id = role.Id });
    }

    [HttpPost]
    [Route("{id}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            SetErrorMessage("Role not found.");
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(role.Name, RoleConstants.Administrator, StringComparison.OrdinalIgnoreCase))
        {
            SetErrorMessage("The Administrator role cannot be deleted.");
            return RedirectToAction(nameof(Index));
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
        if (usersInRole.Count > 0)
        {
            SetErrorMessage($"Role \"{role.Name}\" cannot be deleted because {usersInRole.Count} user(s) are assigned to it.");
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            _logger.LogInformation("Role {Role} deleted by {Admin}", role.Name, User.Identity?.Name);
            SetSuccessMessage($"Role \"{role.Name}\" has been deleted.");
        }
        else
        {
            AddIdentityErrors(result);
            SetErrorMessage("Failed to delete the role. See form errors for details.");
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Replaces the permission claims on a role with the given set.
    /// </summary>
    private async Task ReplacePermissionsAsync(IdentityRole role, IReadOnlyList<string> selected)
    {
        var current = (await _roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == PermissionClaimType)
            .ToList();

        var toRemove = current.Where(c => !selected.Contains(c.Value)).ToList();
        foreach (var claim in toRemove)
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        var existingValues = current.Select(c => c.Value).ToHashSet();
        foreach (var permission in selected)
        {
            if (existingValues.Contains(permission))
            {
                continue;
            }

            await _roleManager.AddClaimAsync(
                role,
                new System.Security.Claims.Claim(PermissionClaimType, permission));
        }
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
            _logger.LogWarning("Identity error for role update: {Error}", error.Description);
        }
    }
}
