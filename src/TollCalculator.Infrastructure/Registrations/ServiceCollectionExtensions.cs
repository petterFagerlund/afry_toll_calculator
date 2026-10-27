using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TollCalculator.Infrastructure.Persistence;
using TollCalculator.Domain.Repositories;
using TollCalculator.Infrastructure.Repositories.Implementations;

namespace TollCalculator.Infrastructure.Registrations;

public static class ServiceCollectionExtensions
{
    public const string ConnectionStringName = "TollDatabase";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is missing from configuration.");
        }

        services.AddDbContext<TollDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ITollPassageRepository, TollPassageRepository>();
        services.AddScoped<ITollScheduleRepository, TollScheduleRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IVehicleTypeRuleRepository, VehicleTypeRuleRepository>();

        return services;
    }
}
