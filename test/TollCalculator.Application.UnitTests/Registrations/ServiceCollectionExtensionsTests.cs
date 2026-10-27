using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TollCalculator.Application.Registrations;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Services;
using TollCalculator.Infrastructure.Repositories;
using Xunit;

namespace TollCalculator.Application.UnitTests.Registrations;

public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(typeof(ITollPassageService))]
    [InlineData(typeof(ITollService))]
    [InlineData(typeof(ITollRateService))]
    [InlineData(typeof(IHolidayService))]
    [InlineData(typeof(IVehicleTypeService))]
    [InlineData(typeof(IDailyTollServiceFactory))]
    public void AddApplication_ResolvesRegisteredService(Type serviceType)
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService(serviceType));
    }

    [Fact]
    public void AddApplication_RegistersServicesAsScoped()
    {
        using var provider = BuildProvider();

        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var first = firstScope.ServiceProvider.GetRequiredService<ITollService>();

        Assert.Same(first, firstScope.ServiceProvider.GetRequiredService<ITollService>());
        Assert.NotSame(first, secondScope.ServiceProvider.GetRequiredService<ITollService>());
    }

    [Fact]
    public void AddApplication_RegistersAFactoryThatComposesTheDomainServices()
    {
        using var provider = BuildProvider();

        var factory = provider.GetRequiredService<IDailyTollServiceFactory>();

        Assert.IsAssignableFrom<IDailyTollService>(factory.Create([]));
    }

    [Fact]
    public void AddApplication_ThrowsWhenServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddApplication());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton(Substitute.For<IVehicleRepository>());
        services.AddSingleton(Substitute.For<ITollPassageRepository>());
        services.AddSingleton(Substitute.For<ITollScheduleRepository>());
        services.AddSingleton(Substitute.For<IHolidayRepository>());
        services.AddSingleton(Substitute.For<IVehicleTypeRuleRepository>());

        services.AddApplication();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });
    }
}
