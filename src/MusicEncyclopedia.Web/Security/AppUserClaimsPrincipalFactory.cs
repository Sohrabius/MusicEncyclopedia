using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MusicEncyclopedia.Web.Security;

/// <summary>
/// Extends the default Identity claims factory so that permission claims stored on
/// roles are surfaced directly on the signed-in user's principal. This makes
/// role-based permission grants (managed in the admin Roles section) satisfy the
/// application's permission policies, which use <c>RequireClaim("Permission", ...)</c>.
/// Direct user permission claims continue to work unchanged.
/// </summary>
public sealed class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
{
    public AppUserClaimsPrincipalFactory(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var roleNames = await UserManager.GetRolesAsync(user);
        foreach (var roleName in roleNames)
        {
            var role = await RoleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            foreach (var claim in await RoleManager.GetClaimsAsync(role))
            {
                // Skip claims the base factory already emitted (e.g. the role claim
                // itself) and surface everything else — notably "Permission".
                if (identity.HasClaim(claim.Type, claim.Value))
                {
                    continue;
                }

                identity.AddClaim(new Claim(claim.Type, claim.Value));
            }
        }

        return identity;
    }
}
