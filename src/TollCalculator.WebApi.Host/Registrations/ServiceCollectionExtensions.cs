using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Host.Mappers.Implementations;

namespace TollCalculator.WebApi.Host.Registrations;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMappers(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ITollPassageMapper, TollPassageMapper>();
        services.AddSingleton<IDailyTollMapper, DailyTollMapper>();
        services.AddSingleton<ITollRateMapper, TollRateMapper>();
        services.AddSingleton<IHolidayMapper, HolidayMapper>();
        services.AddSingleton<IVehicleTypeMapper, VehicleTypeMapper>();

        return services;
    }
}
