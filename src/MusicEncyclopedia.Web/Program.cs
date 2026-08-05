using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using HealthChecks.SqlServer;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Serilog;
using AspNetCoreRateLimit;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Seed;
using MusicEncyclopedia.Web.Middleware;
using MusicEncyclopedia.Web.Filters;

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

    // ---- Database Provider Detection ----
    var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
    var isSqlite = string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase);

    string connectionString;
    if (isSqlite)
    {
        connectionString = builder.Configuration.GetConnectionString("SqliteConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'SqliteConnection' not found. " +
                "Ensure it is configured in appsettings.Development.json.");
    }
    else
    {
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. " +
                "Ensure it is configured in appsettings.json or environment variables.");
    }

    // ---- Database Context & Data Services ----
    builder.Services.AddDataServices(connectionString, useSqlite: isSqlite);

    // Register Dapper IDbConnection for admin dashboard and other quick queries
    if (isSqlite)
    {
        builder.Services.AddScoped<IDbConnection>(_ => new SqliteConnection(connectionString));
    }
    else
    {
        builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));
    }

    // ---- Application Services ----
    // Register services from MusicEncyclopedia.Services, .Search, .Media projects.
    builder.Services.AddServices(isSqlite);
    builder.Services.AddSearchServices(isSqlite);
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

    // ---- Authentication Cookie Configuration ----
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/access-denied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
    if (!isSqlite)
    {
        healthChecksBuilder.AddSqlServer(
            connectionString: connectionString,
            name: "sql-server",
            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
            tags: ["db", "sql", "ready"]);
    }

    // ---- Response Caching ----
    builder.Services.AddResponseCaching();

    // ---- Rate Limiting (Section 17.5) ----
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

    // ---- Hangfire Background Jobs (Section 20) ----
    // Hangfire requires SQL Server; skip in SQLite mode
    if (!isSqlite)
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
    }

    // ---- Anti-Forgery Tokens (Section 17.3) ----
    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-CSRF-TOKEN";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

    // ---- Memory Cache & Distributed Cache ----
    builder.Services.AddMemoryCache();

    // ────────────────────────────────────────────────────────────
    // Middleware Pipeline
    // ────────────────────────────────────────────────────────────
    var app = builder.Build();

    // Database initialization for SQLite (create schema + seed lookup data)
    if (isSqlite)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DatabaseInitializer.InitializeAsync(context, isSqlite);
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

    // ---- HTTPS Redirection (enforce in production) ----
    if (!app.Environment.IsDevelopment())
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

    // ---- Response Caching ----
    app.UseResponseCaching();

    // ---- Routing ----
    app.UseRouting();

    // ---- Request Localization (Culture from route) ----
    app.UseRequestLocalization(options =>
    {
        var supportedCultures = new[]
        {
            new CultureInfo("fa")
        };

        options.DefaultRequestCulture = new RequestCulture("fa");
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

    // ---- Conventional Routes (Section 28) ----
    app.MapControllerRoute(
        name: "localized-default",
        pattern: "{culture:regex(^(fa)$)}/{controller=Home}/{action=Index}/{id?}");

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


