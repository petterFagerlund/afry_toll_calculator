using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TollCalculator.Application.Results;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Controllers.V1;
using TollCalculator.WebApi.Host.Mappers.Implementations;
using TollCalculator.WebApi.Models.V1.Responses;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Controllers;

public class TollRatesControllerTests
{
    private readonly ITollRateService _tollRateService = Substitute.For<ITollRateService>();
    private readonly TollRatesController _controller;

    public TollRatesControllerTests()
    {
        _controller = new TollRatesController(_tollRateService, new TollRateMapper());
    }

    [Fact]
    public async Task GetTollRates_ReturnsTheScheduleIntervals()
    {
        var interval = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), 8);
        _tollRateService.GetTollRatesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<TollRateInterval[]>.Success([interval]));

        var result = await _controller.GetTollRates(CancellationToken.None);

        var responses = Assert.IsType<TollRateResponse[]>(Assert.IsType<OkObjectResult>(result.Result).Value);
        var response = Assert.Single(responses);
        Assert.Equal("06:00", response.Start);
        Assert.Equal("06:29", response.End);
        Assert.Equal(8, response.Amount);
    }

    [Fact]
    public async Task GetTollRates_ReturnsBadRequestWhenTheServiceFails()
    {
        _tollRateService.GetTollRatesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<TollRateInterval[]>.Invalid("The schedule is unavailable."));

        var result = await _controller.GetTollRates(CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }
}

public class HolidaysControllerTests
{
    private readonly IHolidayService _holidayService = Substitute.For<IHolidayService>();
    private readonly HolidaysController _controller;

    public HolidaysControllerTests()
    {
        _controller = new HolidaysController(_holidayService, new HolidayMapper());
    }

    [Fact]
    public async Task GetHolidays_ReturnsEveryHolidayWhenNoYearIsGiven()
    {
        var holiday = Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen");
        _holidayService.GetHolidaysAsync(null, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<Holiday[]>.Success([holiday]));

        var result = await _controller.GetHolidays(null, CancellationToken.None);

        var responses = Assert.IsType<HolidayResponse[]>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("Nyårsdagen", Assert.Single(responses).Name);
    }

    [Fact]
    public async Task GetHolidays_PassesTheRequestedYearToTheService()
    {
        _holidayService.GetHolidaysAsync(2027, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<Holiday[]>.Success([]));

        var result = await _controller.GetHolidays(2027, CancellationToken.None);

        Assert.Empty(Assert.IsType<HolidayResponse[]>(Assert.IsType<OkObjectResult>(result.Result).Value));
        await _holidayService.Received(1).GetHolidaysAsync(2027, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetHolidays_ReturnsBadRequestForAnImpossibleYear()
    {
        _holidayService.GetHolidaysAsync(0, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<Holiday[]>.Invalid("The year 0 is outside the supported range 1-9999."));

        var result = await _controller.GetHolidays(0, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.Equal("The year 0 is outside the supported range 1-9999.",
            Assert.IsType<ProblemDetails>(problem.Value).Detail);
    }
}

public class VehicleTypesControllerTests
{
    private readonly IVehicleTypeService _vehicleTypeService = Substitute.For<IVehicleTypeService>();
    private readonly VehicleTypesController _controller;

    public VehicleTypesControllerTests()
    {
        _controller = new VehicleTypesController(_vehicleTypeService, new VehicleTypeMapper());
    }

    [Fact]
    public async Task GetVehicleTypes_ReturnsEveryRuleWithItsTollFreeFlag()
    {
        var car = VehicleTypeRule.Create(VehicleType.Car, isTollFree: false);
        var motorbike = VehicleTypeRule.Create(VehicleType.Motorbike, isTollFree: true);
        _vehicleTypeService.GetVehicleTypeRulesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<VehicleTypeRule[]>.Success([car, motorbike]));

        var result = await _controller.GetVehicleTypes(CancellationToken.None);

        var responses = Assert.IsType<VehicleTypeResponse[]>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(["Car", "Motorbike"], responses.Select(response => response.VehicleType));
        Assert.Equal([false, true], responses.Select(response => response.IsTollFree));
    }

    [Fact]
    public async Task GetVehicleTypes_ReturnsBadRequestWhenTheServiceFails()
    {
        _vehicleTypeService.GetVehicleTypeRulesAsync(Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<VehicleTypeRule[]>.Invalid("The vehicle type rules are unavailable."));

        var result = await _controller.GetVehicleTypes(CancellationToken.None);

        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsType<ObjectResult>(result.Result).StatusCode);
    }
}
