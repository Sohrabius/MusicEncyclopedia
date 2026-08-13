using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Search.Services;

namespace MusicEncyclopedia.Search;

/// <summary>
/// Extension methods for registering search services in the dependency injection container.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers the search-related services.
    /// When <paramref name="isSqlite"/> is true, registers a LIKE-based search service
    /// that works with SQLite; otherwise registers a FREETEXTTABLE-based service for SQL Server.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="isSqlite">If true, register SQLite-compatible search service.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    public static IServiceCollection AddSearchServices(this IServiceCollection services, bool isSqlite = false)
    {
        services.AddScoped<ISearchService>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var dbProvider = configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
            var useSqlite = isSqlite || string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase);

            // NOTE: Program.cs injects the DbPassword secret into
            // "ConnectionStrings:DefaultConnection" at startup (ConfigurationManager
            // override), so every consumer — including this service — reads the
            // password-ready string from IConfiguration.
            var connectionString = useSqlite
                ? configuration.GetConnectionString("SqliteConnection")
                    ?? throw new InvalidOperationException("Connection string 'SqliteConnection' not found in configuration.")
                : configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

            return new SearchService(connectionString, useSqlite, sp.GetService<ICacheService>());
        });

        return services;
    }

    /// <summary>
    /// Registers the search-related services using a pre-resolved connection string.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="connectionString">The connection string to use.</param>
    /// <param name="isSqlite">If true, use SQLite-compatible search queries.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    public static IServiceCollection AddSearchServices(
        this IServiceCollection services,
        string connectionString,
        bool isSqlite = false)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddScoped<ISearchService>(_ => new SearchService(connectionString, isSqlite));

        return services;
    }
}
