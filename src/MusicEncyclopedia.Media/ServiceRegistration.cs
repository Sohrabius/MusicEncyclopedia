using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicEncyclopedia.Media.Interfaces;
using MusicEncyclopedia.Media.Services;

namespace MusicEncyclopedia.Media;

/// <summary>
/// Extension methods for registering media-related services into the DI container.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers media services including <see cref="IMediaService"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration (used for <c>Media:StoragePath</c> and <c>Media:CdnBaseUrl</c>).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediaServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind media settings (optional, for strongly typed access).
        // The MediaService reads configuration directly via IConfiguration.
        services.AddScoped<IMediaService, MediaService>();

        return services;
    }
}
