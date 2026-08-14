using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Services.Infrastructure;
using MusicEncyclopedia.Services.Services;
using MusicEncyclopedia.Services.Validators;

namespace MusicEncyclopedia.Services;

/// <summary>
/// Extension methods for registering service-layer dependencies.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers all application services and validators for the MusicEncyclopedia.Services project.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Register Dapper-based query services optimized for SQL Server
        services.AddScoped<IAlbumQueryService, AlbumQueryService>();
        services.AddScoped<ITrackQueryService, TrackQueryService>();
        services.AddScoped<IPersonQueryService, PersonQueryService>();
        services.AddScoped<ICompanyQueryService, CompanyQueryService>();

        // Business services (scoped)
        services.AddScoped<ICreditService, CreditService>();
        services.AddScoped<IContentLocalizationService, ContentLocalizationService>();

        // Utility services (singleton - stateless)
        services.AddSingleton<ISlugService, SlugService>();
        services.AddSingleton<ICacheService, CacheService>();

        // Cache invalidation infrastructure
        services.AddSingleton<CacheKeyTracker>();
        services.AddScoped<CacheInvalidationService>();

        // File upload validation (singleton - stateless configuration)
        services.AddSingleton<FileValidationService>();

        // Register all FluentValidation validators from this assembly
        services.AddValidatorsFromAssemblyContaining<AlbumValidator>();

        return services;
    }
}
