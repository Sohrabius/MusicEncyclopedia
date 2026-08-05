using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, bool isSqlite = false)
    {
        MusicEncyclopedia.Services.ServiceRegistration.AddServices(services, isSqlite);
        return services;
    }

    public static IServiceCollection AddSearchServices(this IServiceCollection services, bool isSqlite = false)
    {
        MusicEncyclopedia.Search.ServiceRegistration.AddSearchServices(services, isSqlite);
        return services;
    }

    public static IServiceCollection AddMediaServices(this IServiceCollection services)
    {
        services.AddScoped<MusicEncyclopedia.Media.Interfaces.IMediaService, MusicEncyclopedia.Media.Services.MediaService>();
        return services;
    }
}
