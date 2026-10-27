using Microsoft.Extensions.DependencyInjection;
using TollCalculator.Domain.Models;
using NSubstitute;
using TollCalculator.Application.Registrations;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Services;
using TollCalculator.Domain.Repositories;
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
    [InlineData(typeof(IDailyTollService))]
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
    public void AddApplication_ComposesTheDomainServices()
    {
        using var provider = BuildProvider();

        var dailyTollService = provider.GetRequiredService<IDailyTollService>();
        var vehicle = Vehicle.Unregistered(Domain.Enums.VehicleType.Car);
        var tariff = TollTariff.Create(TollSchedule.Create([]), 60);

        Assert.Equal(0, dailyTollService.Calculate(vehicle, [], tariff, []).TotalFee);
    }

    [Fact]
    public void AddApplication_RegistersTheDomainServicesAsSingletons()
    {
        using var provider = BuildProvider();

        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        Assert.Same(
            firstScope.ServiceProvider.GetRequiredService<IDailyTollService>(),
            secondScope.ServiceProvider.GetRequiredService<IDailyTollService>());
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
