using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using HealthChecks.SqlServer;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Data.SqlClient;
using Serilog;
using AspNetCoreRateLimit;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Seed;
using MusicEncyclopedia.Web.Middleware;
using MusicEncyclopedia.Web.Filters;
using MusicEncyclopedia.Web.Jobs;
using MusicEncyclopedia.Web.Security;
using MusicEncyclopedia.Web.Seed;

// ────────────────────────────────────────────────────────────────
// Serilog Bootstrapping
// ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        buffered: false)
    .CreateLogger();

try
{
    Log.Information("Starting Music Encyclopedia Web application");

    // ────────────────────────────────────────────────────────────
    // Web Application Builder
    // ────────────────────────────────────────────────────────────
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog as the logging provider
    builder.Host.UseSerilog();

    // ────────────────────────────────────────────────────────────
    // Service Registration
    // ────────────────────────────────────────────────────────────

    // ---- Database Connection ----
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' not found. " +
            "Ensure it is configured in appsettings.json or environment variables.");

    // The database password is a secret and must never live in appsettings
    // files. Read it from configuration instead — env var `DbPassword`
    // (or `DB_PASSWORD`) in Production, or ASP.NET Core user secrets in
    // Development — and inject it into the connection string.
    // A fully-specified ConnectionStrings__DefaultConnection override that
    // already contains Password= wins untouched.
    var dbPassword = builder.Configuration["DbPassword"];// "Ms*6802951";
    if (!string.IsNullOrWhiteSpace(dbPassword) &&
        !connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase))
    {
        var csb = new SqlConnectionStringBuilder(connectionString)
        {
            Password = dbPassword
        };
        connectionString = csb.ConnectionString;
    }

    // Override the configuration value so every consumer that reads
    // DefaultConnection from IConfiguration at resolve time (Dapper query
    // services in MusicEncyclopedia.Services, the search service, and the
    // admin/API controllers) receives the password-ready connection string.
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

    // ---- Database Context & Data Services ----
    builder.Services.AddDataServices(connectionString);

    // Register Dapper IDbConnection for admin dashboard and other quick queries
    builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));

    // SQL Server returns date/datetime2 columns as DateTime; without a
    // registered handler Dapper falls back to Convert.ChangeType, which
    // cannot cast DateTime to DateOnly (queries reading ReleaseDate and
    // other date columns fail with InvalidCastException). Register an
    // explicit handler that converts DateTime -> DateOnly on read and
    // writes DateOnly as datetime2 on write.
    Dapper.SqlMapper.AddTypeHandler(new SqlServerDateOnlyHandler());

    // ---- Application Services ----
    // Register services from MusicEncyclopedia.Services, .Search, .Media projects.
    builder.Services.AddServices();
    builder.Services.AddSearchServices();
    builder.Services.AddMediaServices();

    // ---- ASP.NET Core Identity ----
    builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Password rules per spec Section 17.1
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        // Sign-in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>();

    // Surface permission claims stored on roles on the signed-in user's principal
    // (role-based permissions satisfy the application's permission policies).
    builder.Services.AddScoped<
        IUserClaimsPrincipalFactory<IdentityUser>,
        AppUserClaimsPrincipalFactory>();

    // ---- Authentication Cookie Configuration ----
    // CookieSecurePolicy.Always breaks local HTTP development (and the login page),
    // so only enforce Secure cookies outside Development.
    var isDevelopment = builder.Environment.IsDevelopment();
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/access-denied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

    // ---- Authorization Policies ----
    // Register all 21 permission policies from PermissionConstants (Section 10.2)
    builder.Services.AddAuthorization(options =>
    {
        foreach (var policy in PermissionConstants.All)
        {
            options.AddPolicy(policy, policyBuilder =>
                policyBuilder.RequireClaim("Permission", policy));
        }

        // Special policy: admin area requires any admin permission
        options.AddPolicy("AdminArea", policyBuilder =>
            policyBuilder.RequireAssertion(context =>
                PermissionConstants.All.Any(p => context.User.HasClaim("Permission", p))));
    });

    // ---- Admin audit logging (global filter) ----
    builder.Services.AddScoped<MusicEncyclopedia.Web.Filters.AuditLogFilter>();

    // ---- Culture-Aware MVC ----
    builder.Services.AddCultureAwareMvc();

    // ---- CORS for API ----
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ApiCors", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });

        options.AddPolicy("RestrictedCors", policy =>
        {
            policy.AllowCredentials()
                  .SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // ---- Health Checks (Section 23) ----
    var healthChecksBuilder = builder.Services.AddHealthChecks();
    healthChecksBuilder.AddSqlServer(
        connectionString: connectionString,
        name: "sql-server",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: ["db", "sql", "ready"]);

    // ---- Rate Limiting (Section 17.5) ----
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

    // ---- Hangfire Background Jobs (Section 20) ----
    // Tests and maintenance tools can disable workers without changing the
    // production default. This keeps disposable database hosts deterministic.
    var backgroundJobsEnabled = builder.Configuration.GetValue("BackgroundJobs:Enabled", true);
    if (backgroundJobsEnabled)
    {
        builder.Services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
            });
        });
        builder.Services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = ["default", "media", "search"];
        });

        // Recurring jobs (spec §20) are resolved from DI by Hangfire.
        builder.Services.AddScoped<CacheWarmJob>();
    }

    // ---- Anti-Forgery Tokens (Section 17.3) ----
    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-CSRF-TOKEN";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

    // ---- Memory Cache & Distributed Cache ----
    builder.Services.AddMemoryCache();

    // ────────────────────────────────────────────────────────────
    // Middleware Pipeline
    // ────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ---- Forwarded Headers (reverse proxy deployments) ----
    // When running behind nginx / Caddy / a cloud load balancer, honor
    // X-Forwarded-For and X-Forwarded-Proto so rate limiting keys off the real
    // client IP and generated links / HTTPS redirect use the external scheme.
    // Gated by Site:BehindProxy (default false) — never trust client-supplied
    // forwarded headers when the app is directly exposed.
    if (builder.Configuration.GetValue("Site:BehindProxy", false))
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            // KnownProxies/KnownNetworks keep their loopback defaults — see
            // DEPLOYMENT.md for restricting to specific proxy addresses.
        });
    }

    // ---- Database initialization ----
    // SQL Server: EF migrations are applied (idempotent) before seeding.
    // Controlled by Seed:OnStartup (default true) and Seed:SampleContent
    // (default: true for the dev path, false in production so production
    // deploys seed lookup data only — set explicitly in appsettings.Production.json).
    var seedOnStartup = builder.Configuration.GetValue("Seed:OnStartup", true);
    var seedSampleContent = builder.Configuration.GetValue("Seed:SampleContent", isDevelopment);

    if (seedOnStartup)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await DatabaseInitializer.InitializeAsync(context, seedSampleContent);
    }

    // ---- First-run admin bootstrap ----
    // Seeds roles + Administrator permissions and creates an admin user:
    // Admin:Email/Admin:Password when configured, otherwise the built-in seed
    // default (admin@example.com) on the dev path. Seed:AdminUser defaults to
    // true for dev (convenience) and false in production (which must supply
    // Admin:Email/Admin:Password). Fully idempotent.
    var adminBootstrapEnabled = builder.Configuration.GetValue("Admin:BootstrapEnabled", true);
    try
    {
        if (adminBootstrapEnabled)
        {
            var seedDefaultAdmin = builder.Configuration.GetValue("Seed:AdminUser", isDevelopment);
            var bootstrapLogger = app.Services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("AdminBootstrap");
            await AdminBootstrap.SeedRolesAndAdminAsync(
                app.Services, builder.Configuration, bootstrapLogger, seedDefaultAdmin);
        }
    }
    catch (Exception ex)
    {
        // Never take the site down because identity seeding hiccupped — log it.
        Log.Warning(ex, "Admin bootstrap did not complete");
    }

    // ---- Exception Handling ----
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Errors/Error");
        app.UseHsts();
    }

    // ---- HTTPS Redirection ----
    // Enabled by default outside Development; explicit opt-out for proxy
    // deployments that terminate TLS upstream (Site:EnableHttpsRedirection=false).
    var enableHttpsRedirection = builder.Configuration.GetValue(
        "Site:EnableHttpsRedirection", !app.Environment.IsDevelopment());
    if (enableHttpsRedirection)
    {
        app.UseHttpsRedirection();
    }

    // ---- Security Headers Middleware (Section 17.4) ----
    app.Use(async (context, next) =>
    {
        // Content-Security-Policy
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com https://cdnjs.cloudflare.com; " +
            "style-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com https://cdnjs.cloudflare.com; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' https:; " +
            "connect-src 'self'; " +
            "media-src 'self' https:; " +
            "frame-src 'none'; " +
            "object-src 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'; " +
            "frame-ancestors 'none'");

        // X-Content-Type-Options: prevent MIME sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options: prevent clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Referrer-Policy
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy
        context.Response.Headers.Append(
            "Permissions-Policy",
            "camera=(), microphone=(), geolocation=(), interest-cohort=()");

        await next();
    });

    // ---- Static Files ----
    app.UseStaticFiles();

    // ---- Rate Limiting Middleware (Section 17.5) ----
    app.UseIpRateLimiting();

    // ---- CORS ----
    app.UseCors("ApiCors");

    // ---- Routing ----
    app.UseRouting();

    // ---- Request Localization (Culture from route) ----
    app.UseRequestLocalization(options =>
    {
        var supportedCultures = CultureConstants.SupportedCultures
            .Select(c => new CultureInfo(c))
            .ToArray();

        options.DefaultRequestCulture = new RequestCulture(CultureConstants.DefaultCulture);
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
        options.RequestCultureProviders.Clear();
    });

    // ---- Custom Culture Middleware ----
    // Reads culture from route data, sets Thread culture and HttpContext.Items
    app.UseMiddleware<CultureMiddleware>();

    // ---- Authentication & Authorization ----
    app.UseAuthentication();
    app.UseAuthorization();

    // ---- Hangfire Dashboard & Recurring Jobs ----
    if (backgroundJobsEnabled)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireDashboardAuthorizationFilter()]
        });

        // Nightly cache warm-up (spec §20 / §15) so the public site starts the
        // day with warm list caches. Registration is best-effort: on first deploy
        // the database may not exist yet and the job is registered on next start.
        try
        {
            RecurringJob.AddOrUpdate<CacheWarmJob>(
                "cache-warm",
                job => job.WarmAsync(),
                Cron.Daily(3, 0));
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to register Hangfire recurring jobs");
        }
    }

    // ---- Conventional Routes (Section 28) ----
    app.MapControllerRoute(
        name: "localized-default",
        pattern: "{culture:regex(^(fa|en|ar|fr)$)}/{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

    // ---- Health Check Endpoints (Section 23) ----
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready") || check.Tags.Contains("db"),
        ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
    });

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false, // light check — no dependencies
        ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
    });

    // Run the application
    Log.Information("Application configured, starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// ────────────────────────────────────────────────────────────────
// Health Check Response Writer
// ────────────────────────────────────────────────────────────────

/// <summary>
/// Dapper type handler that maps SQL Server date/datetime2 values (returned by
/// ADO.NET as <see cref="DateTime"/>) to <see cref="DateOnly"/>.
/// </summary>
internal sealed class SqlServerDateOnlyHandler : Dapper.SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) => value switch
    {
        DateOnly date => date,
        DateTime dateTime => DateOnly.FromDateTime(dateTime),
        string text when DateOnly.TryParse(text, CultureInfo.InvariantCulture, out var parsed) => parsed,
        _ => DateOnly.MinValue
    };

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}

internal static class HealthCheckResponseWriter
{
    /// <summary>
    /// Writes a structured JSON health check response.
    /// </summary>
    internal static async Task WriteResponseAsync(
        HttpContext context,
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                tags = e.Value.Tags,
                exception = e.Value.Exception?.Message
            })
        };

        await System.Text.Json.JsonSerializer.SerializeAsync(
            context.Response.Body,
            response,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
    }
}

// Exposes the top-level entry point to WebApplicationFactory integration tests.
public partial class Program
{
}


