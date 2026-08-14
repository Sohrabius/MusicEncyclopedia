using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MusicEncyclopedia.Data;

/// <summary>
/// Extension methods for registering data-layer services.
/// </summary>
public static class DataServiceRegistration
{
    /// <summary>
    /// Registers the AppDbContext with the SQL Server provider and any data-level services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });

            // Disable lazy loading; use explicit Include/ThenInclude
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        });

        // Register any data-level services here as scoped/transient
        // Example: services.AddScoped<IAlbumRepository, AlbumRepository>();

        return services;
    }

    /// <summary>
    /// Registers the AppDbContext with the SQL Server provider using a connection string
    /// retrieved from configuration. Overload for simpler startup registration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionStringFactory">A factory to provide the connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        Func<string> connectionStringFactory)
    {
        return services.AddDataServices(connectionStringFactory());
    }
}
