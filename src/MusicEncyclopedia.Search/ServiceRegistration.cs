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
    /// Registers the search-related services. Registers a FREETEXTTABLE-based
    /// service for SQL Server (with a LIKE fallback when full-text indexes
    /// are unavailable).
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    public static IServiceCollection AddSearchServices(this IServiceCollection services)
    {
        services.AddScoped<ISearchService>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            // NOTE: Program.cs injects the DbPassword secret into
            // "ConnectionStrings:DefaultConnection" at startup (ConfigurationManager
            // override), so every consumer — including this service — reads the
            // password-ready string from IConfiguration.
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

            return new SearchService(connectionString, sp.GetService<ICacheService>());
        });

        return services;
    }

    /// <summary>
    /// Registers the search-related services using a pre-resolved connection string.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="connectionString">The connection string to use.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    public static IServiceCollection AddSearchServices(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddScoped<ISearchService>(_ => new SearchService(connectionString));

        return services;
    }
}
