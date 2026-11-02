using Microsoft.Extensions.DependencyInjection;
using TollCalculator.Domain.Models;
using NSubstitute;
using TollCalculator.Application.Providers;
using TollCalculator.Application.Registrations;
using TollCalculator.Application.Services;
using TollCalculator.Application.Repositories;

namespace TollCalculator.Application.UnitTests.Registrations;

public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(typeof(ITollPassageService))]
    [InlineData(typeof(ITollService))]
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
    public async Task AddApplication_ComposesTheDomainServices()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var tollScheduleRepository = scope.ServiceProvider.GetRequiredService<ITollScheduleRepository>();
        tollScheduleRepository.GetTariffAsync(Arg.Any<CancellationToken>())
            .Returns(TollTariff.Create(TollSchedule.Create([]), 60));

        var dailyTollService = scope.ServiceProvider.GetRequiredService<IDailyTollService>();

        var result = await dailyTollService.Calculate([], CancellationToken.None);

        Assert.Equal(0, result.Value!.TotalFee);
    }

    [Fact]
    public void AddApplication_RegistersTheDailyTollServiceAsScoped()
    {
        using var provider = BuildProvider();

        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var first = firstScope.ServiceProvider.GetRequiredService<IDailyTollService>();

        Assert.Same(first, firstScope.ServiceProvider.GetRequiredService<IDailyTollService>());
        Assert.NotSame(first, secondScope.ServiceProvider.GetRequiredService<IDailyTollService>());
    }

    [Fact]
    public void AddApplication_ThrowsWhenServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddApplication());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton(Substitute.For<ITollPassageRepository>());
        services.AddScoped(_ => Substitute.For<ITollScheduleRepository>());
        services.AddSingleton(Substitute.For<IHolidayProvider>());

        services.AddApplication();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });
    }
}
