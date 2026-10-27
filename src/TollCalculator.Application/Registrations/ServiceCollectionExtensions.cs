using Microsoft.Extensions.DependencyInjection;
using TollCalculator.Application.Services;
using TollCalculator.Application.Services.Implementations;
using TollCalculator.Domain.Services;
using TollCalculator.Domain.Services.Implementations;

namespace TollCalculator.Application.Registrations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ITollFreeDateService, WeekendAndHolidayService>();
        services.AddSingleton<ITollFeeService, TollFeeService>();
        services.AddSingleton<IDailyTollService, DailyTollService>();

        services.AddScoped<ITollPassageService, TollPassageService>();
        services.AddScoped<ITollService, TollService>();
        services.AddScoped<ITollRateService, TollRateService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IVehicleTypeService, VehicleTypeService>();

        return services;
    }
}
