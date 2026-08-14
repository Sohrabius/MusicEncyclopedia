using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        MusicEncyclopedia.Services.ServiceRegistration.AddServices(services);
        return services;
    }

    public static IServiceCollection AddSearchServices(this IServiceCollection services)
    {
        MusicEncyclopedia.Search.ServiceRegistration.AddSearchServices(services);
        return services;
    }

    public static IServiceCollection AddMediaServices(this IServiceCollection services)
    {
        services.AddScoped<MusicEncyclopedia.Media.Interfaces.IMediaService, MusicEncyclopedia.Media.Services.MediaService>();
        return services;
    }
}
