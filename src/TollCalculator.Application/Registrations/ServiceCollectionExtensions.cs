using Microsoft.Extensions.DependencyInjection;
using TollCalculator.Application.Services;
using TollCalculator.Application.Services.Implementations;

namespace TollCalculator.Application.Registrations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ITollFreeDateService, TollFreeDateService>();
        services.AddSingleton<ITollFeeService, TollFeeService>();
        services.AddScoped<IDailyTollService, DailyTollService>();
        services.AddScoped<ITollPassageService, TollPassageService>();
        services.AddScoped<ITollService, TollService>();

        return services;
    }
}
