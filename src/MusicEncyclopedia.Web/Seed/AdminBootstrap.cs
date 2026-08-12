using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;

namespace MusicEncyclopedia.Web.Seed;

/// <summary>
/// First-run bootstrap that satisfies the "Admin account created / Roles seeded"
/// deployment checklist (spec §22.4). Runs on every startup and is fully
/// idempotent:
///  1. Creates the six roles (Administrator, Editor, Contributor, Reviewer,
///     MediaManager, UserManager) if missing.
///  2. Grants the Administrator role every permission claim so a freshly
///     bootstrapped admin can use the whole admin area without manual setup.
///  3. Creates an admin user and assigns the Administrator role. Precedence:
///     <c>Admin:Email</c> (plus <c>Admin:Password</c>) when configured; otherwise,
///     when <paramref name="seedDefaultAdmin"/> is true (the SQLite/dev path),
///     the built-in seed default (admin@example.com / Admin@123456) so a fresh
///     dev database has a usable administrator with no env setup.
///
/// Each step runs in its own DbContext scope. Permission claims and the user-role
/// link are written through the DbSet directly rather than
/// <c>RoleManager.AddClaimAsync</c>/<c>UserManager.AddToRoleAsync</c>: those
/// attach freshly-created (Added-state) entities back to a context that already
/// tracks them, which throws an EF tracking conflict on a first boot. The
/// direct writes produce the exact same rows the admin Roles screen manages.
///
/// Concurrency: on multi-instance/rolling deploys two instances may race the
/// existence-check-then-insert steps. The unique <c>NormalizedName</c> index makes
/// the losing insert throw; the caller's try/catch logs a warning and the other
/// instance completes, so the state self-heals on the next boot. Bootstrap
/// failures must never become fatal — they are retried automatically on every
/// startup.
/// </summary>
public static class AdminBootstrap
{
    private const string PermissionClaimType = "Permission";

    /// <summary>Built-in seed default administrator (used when no Admin:Email is configured).</summary>
    private const string DefaultAdminEmail = "admin@example.com";

    /// <summary>
    /// Meets the app password policy (10+ chars, digit, upper, lower, symbol).
    /// Only used on the dev/default path — operators should set their own via
    /// Admin:Password or change it after first login.
    /// </summary>
    private const string DefaultAdminPassword = "Admin@123456";

    public static async Task SeedRolesAndAdminAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger,
        bool seedDefaultAdmin)
    {
        // 1) Roles ----------------------------------------------------------
        using (var scope = services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var roleName in RoleConstants.All)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    logger.LogInformation("Seeded role {Role}", roleName);
                }
                else
                {
                    logger.LogWarning("Failed to seed role {Role}: {Errors}",
                        roleName, string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        // 2) Administrator gets every permission ----------------------------
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var adminRoleId = await db.Roles
                .Where(r => r.Name == RoleConstants.Administrator)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (adminRoleId is not null)
            {
                var existing = await db.RoleClaims
                    .Where(c => c.RoleId == adminRoleId && c.ClaimType == PermissionClaimType)
                    .Select(c => c.ClaimValue)
                    .ToListAsync();

                var missing = PermissionConstants.All.Where(p => !existing.Contains(p)).ToList();
                foreach (var permission in missing)
                {
                    db.RoleClaims.Add(new IdentityRoleClaim<string>
                    {
                        RoleId = adminRoleId,
                        ClaimType = PermissionClaimType,
                        ClaimValue = permission
                    });
                }

                if (missing.Count > 0)
                {
                    await db.SaveChangesAsync();
                    logger.LogInformation(
                        "Granted {Count} permissions to the Administrator role", missing.Count);
                }
            }
        }

        // 3) Admin user -----------------------------------------------------
        // Precedence: the operator-configured Admin:Email/Admin:Password wins;
        // when those are absent and seedDefaultAdmin is enabled (the dev/SQLite
        // path, config Seed:AdminUser), fall back to the built-in seed default
        // so a fresh dev database has a usable administrator out of the box.
        var adminEmail = configuration["Admin:Email"];
        var password = configuration["Admin:Password"];
        var usesSeedDefault = false;

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            if (!seedDefaultAdmin)
            {
                return; // No admin requested — roles/permissions are still seeded.
            }

            adminEmail = configuration["Seed:AdminEmail"] ?? DefaultAdminEmail;
            password = configuration["Seed:AdminPassword"] ?? DefaultAdminPassword;
            usesSeedDefault = true;
        }

        using (var scope = services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser is null)
            {
                if (string.IsNullOrEmpty(password))
                {
                    logger.LogWarning(
                        "Admin:Email is set but Admin:Password is missing — skipping admin user creation.");
                    return;
                }

                if (usesSeedDefault)
                {
                    logger.LogWarning(
                        "No Admin:Email configured — seeding the built-in default administrator " +
                        "{Email}. Change the password after first login, or set " +
                        "Seed:AdminUser=false in production.",
                        adminEmail);
                }

                // Username mirrors the email exactly — the login form posts the
                // email and PasswordSignInAsync treats it as the user name (same
                // contract as registration), so a distinct user name would break
                // login for the bootstrapped admin.
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, password);
                if (!createResult.Succeeded)
                {
                    // Common cause: password fails the configured policy. Log loudly
                    // but never take down startup — the operator can create the user
                    // through the admin UI once another admin exists.
                    logger.LogWarning(
                        "Failed to create admin user {Email}: {Errors}",
                        adminEmail, string.Join("; ", createResult.Errors.Select(e => e.Description)));
                    return;
                }

                logger.LogInformation("Created initial administrator {Email}", adminEmail);
            }

            // Role assignment via the link table directly (same rows the admin
            // Users screen would write, without RoleManager attach pitfalls).
            var adminRole = await db.Roles
                .FirstOrDefaultAsync(r => r.Name == RoleConstants.Administrator);
            if (adminRole is not null &&
                !await db.UserRoles.AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id))
            {
                db.UserRoles.Add(new IdentityUserRole<string>
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                await db.SaveChangesAsync();
                logger.LogInformation("Assigned Administrator role to {Email}", adminEmail);
            }
        }
    }
}
