using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Admin user management controller.
/// Route: /admin/users
/// Manages user role membership, permission claims (the claims that actually gate
/// the permission policies), and account lockout state.
/// </summary>
[Route("/admin/users")]
[Authorize(Policy = PermissionConstants.CanManageUsers)]
public sealed class UsersController : AdminBaseController
{
    private const string PermissionClaimType = "Permission";

    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserManager<IdentityUser> userManager,
        ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        int page = 1,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var query = _userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(u => u.Email != null && u.Email.Contains(term));
        }

        var totalItems = await _userManager.Users.CountAsync(u =>
            string.IsNullOrWhiteSpace(q) || (u.Email != null && u.Email.Contains(q.Trim())));

        var users = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<UserListItem>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = (await _userManager.GetClaimsAsync(user))
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToList();
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

            items.Add(new UserListItem
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? user.Id,
                Roles = (IReadOnlyList<string>)roles,
                PermissionCount = permissions.Count,
                IsLockedOut = lockoutEnd is not null && lockoutEnd > DateTimeOffset.UtcNow,
                EmailConfirmed = await _userManager.IsEmailConfirmedAsync(user)
            });
        }

        var viewModel = new UserListViewModel
        {
            Items = PagedResult<UserListItem>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Users";
        ViewData["ActiveMenu"] = "Users";

        return View(viewModel);
    }

    [HttpGet]
    [Route("{id}/Edit")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            SetErrorMessage("User not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = await BuildEditViewModelAsync(user);
        ViewData["Title"] = $"Edit User: {user.Email}";
        ViewData["ActiveMenu"] = "Users";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserEditViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            SetErrorMessage("User ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            SetErrorMessage("User not found.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = $"Edit User: {user.Email}";
            return View(await BuildEditViewModelAsync(user, viewModel));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = viewModel.SelectedRoles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(viewModel.SelectedRoles).ToList();

        if (rolesToAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                AddIdentityErrors(addResult);
            }
        }

        if (rolesToRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                AddIdentityErrors(removeResult);
            }
        }

        // Replace permission claims (the claims the authorization policies read).
        var currentPermissions = (await _userManager.GetClaimsAsync(user))
            .Where(c => c.Type == PermissionClaimType)
            .ToList();
        var selected = viewModel.SelectedPermissions ?? [];
        var toAdd = selected.Except(currentPermissions.Select(c => c.Value)).ToList();
        var toRemove = currentPermissions.Where(c => !selected.Contains(c.Value)).ToList();

        if (toRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveClaimsAsync(user, toRemove);
            if (!removeResult.Succeeded)
            {
                AddIdentityErrors(removeResult);
            }
        }

        if (toAdd.Count > 0)
        {
            var addResult = await _userManager.AddClaimsAsync(
                user,
                toAdd.Select(p => new System.Security.Claims.Claim(PermissionClaimType, p)));
            if (!addResult.Succeeded)
            {
                AddIdentityErrors(addResult);
            }
        }

        // Lockout state
        await _userManager.SetLockoutEndDateAsync(
            user,
            viewModel.IsLockedOut ? DateTimeOffset.MaxValue : null);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = $"Edit User: {user.Email}";
            return View(await BuildEditViewModelAsync(user, viewModel));
        }

        _logger.LogInformation(
            "User {Email} updated by {Admin}: roles {Roles}, permissions {PermissionCount}",
            user.Email, User.Identity?.Name, rolesToAdd.Count - rolesToRemove.Count, toAdd.Count);

        SetSuccessMessage($"User \"{user.Email}\" updated successfully.");
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [Route("{id}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            SetErrorMessage("User not found.");
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(user.Email, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
        {
            SetErrorMessage("You cannot delete your own account.");
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} deleted by {Admin}", user.Email, User.Identity?.Name);
            SetSuccessMessage($"User \"{user.Email}\" has been deleted.");
        }
        else
        {
            AddIdentityErrors(result);
            SetErrorMessage("Failed to delete the user. See form errors for details.");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<UserEditViewModel> BuildEditViewModelAsync(IdentityUser user, UserEditViewModel? existing = null)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = (await _userManager.GetClaimsAsync(user))
            .Where(c => c.Type == PermissionClaimType)
            .Select(c => c.Value)
            .ToList();
        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

        return new UserEditViewModel
        {
            Id = user.Id,
            Email = user.Email ?? user.UserName ?? user.Id,
            EmailConfirmed = await _userManager.IsEmailConfirmedAsync(user),
            IsLockedOut = existing?.IsLockedOut
                ?? (lockoutEnd is not null && lockoutEnd > DateTimeOffset.UtcNow),
            AllRoles = RoleConstants.All,
            SelectedRoles = existing?.SelectedRoles ?? (IReadOnlyList<string>)roles,
            AllPermissions = PermissionConstants.All,
            SelectedPermissions = existing?.SelectedPermissions ?? (IReadOnlyList<string>)permissions
        };
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
            _logger.LogWarning("Identity error for user update: {Error}", error.Description);
        }
    }
}
