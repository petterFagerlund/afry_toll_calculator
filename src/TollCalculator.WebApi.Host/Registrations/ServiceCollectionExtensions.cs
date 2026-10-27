using Asp.Versioning;
using TollCalculator.WebApi.Host.Configuration;
using TollCalculator.WebApi.Host.ErrorHandling.Implementations;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Host.Mappers.Implementations;
using TollCalculator.WebApi.Host.Validators;
using TollCalculator.WebApi.Host.Validators.Implementation;

namespace TollCalculator.WebApi.Host.Registrations;

internal static class ServiceCollectionExtensions
{
    private const string VersionGroupNameFormat = "'v'VVV";

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

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IRecordTollPassageRequestValidator, RecordTollPassageRequestValidator>();
        services.AddSingleton<IDailyTollQueryValidator, DailyTollQueryValidator>();

        return services;
    }

    public static IServiceCollection AddJwtOptions(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddVersionedApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = VersionGroupNameFormat;
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }

    public static IServiceCollection AddProblemDetailsErrorHandling(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails();
        services.AddExceptionHandler<UnhandledExceptionHandler>();

        return services;
    }
}
