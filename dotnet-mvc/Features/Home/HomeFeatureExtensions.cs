using DotnetMvc.Features.Home.Infra;
using DotnetMvc.Features.Home.Services;

namespace DotnetMvc.Features.Home;

public static class HomeFeatureExtensions
{
    public static IServiceCollection AddHomeFeature(this IServiceCollection services)
    {
        services.AddScoped<IHomeService, HomeService>();
        services.AddSingleton<IApplicationInfoRepository, ApplicationInfoRepository>();
        return services;
    }
}
