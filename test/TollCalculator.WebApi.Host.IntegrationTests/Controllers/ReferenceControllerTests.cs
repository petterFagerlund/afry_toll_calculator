using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TollCalculator.Application.Results;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Controllers.V1;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Models.V1.Responses;
using Xunit;
using ContractVehicleType = TollCalculator.WebApi.Models.V1.VehicleType;

namespace TollCalculator.WebApi.Host.IntegrationTests.Controllers;

public class TollRatesControllerTests
{
    private readonly ITollRateService _tollRateService = Substitute.For<ITollRateService>();
    private readonly TollRatesController _controller;

    public TollRatesControllerTests()
    {
        _controller = new TollRatesController(_tollRateService);
    }

    [Fact]
    public async Task GetTollRates_ReturnsTheScheduleIntervals()
    {
        var interval = TollRateInterval.Create(new TimeOnly(6, 0), new TimeOnly(6, 29), 8);
        _tollRateService.GetTollRatesAsync(Arg.Any<CancellationToken>()).Returns([interval]);

        var result = await _controller.GetTollRates(CancellationToken.None);

        var responses = Assert.IsType<TollRateResponse[]>(Assert.IsType<OkObjectResult>(result.Result).Value);
        var response = Assert.Single(responses);
        Assert.Equal("06:00", response.Start);
        Assert.Equal("06:29", response.End);
        Assert.Equal(8, response.Amount);
    }

}

public class HolidaysControllerTests
{
    private readonly IHolidayService _holidayService = Substitute.For<IHolidayService>();
    private readonly HolidaysController _controller;

    public HolidaysControllerTests()
    {
        _controller = new HolidaysController(_holidayService);
    }

    [Fact]
    public void GetHolidays_ReturnsTheCurrentYearWhenNoYearIsGiven()
    {
        var holiday = Holiday.Create(new DateOnly(2026, 1, 1), "Nyårsdagen");
        _holidayService.GetHolidays(null)
            .Returns(ApplicationResult<Holiday[]>.Success([holiday]));

        var result = _controller.GetHolidays(null);

        var responses = Assert.IsType<HolidayResponse[]>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("Nyårsdagen", Assert.Single(responses).Name);
    }

    [Fact]
    public void GetHolidays_PassesTheRequestedYearToTheService()
    {
        _holidayService.GetHolidays(2027)
            .Returns(ApplicationResult<Holiday[]>.Success([]));

        var result = _controller.GetHolidays(2027);

        Assert.Empty(Assert.IsType<HolidayResponse[]>(Assert.IsType<OkObjectResult>(result.Result).Value));
        _holidayService.Received(1).GetHolidays(2027);
    }

    [Fact]
    public void GetHolidays_ReturnsBadRequestForAnImpossibleYear()
    {
        _holidayService.GetHolidays(0)
            .Returns(ApplicationResult<Holiday[]>.Invalid("The year 0 is outside the supported range 1-9999."));

        var result = _controller.GetHolidays(0);

        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        var validationProblem = Assert.IsType<ValidationProblemDetails>(problem.Value);
        Assert.Equal("The year 0 is outside the supported range 1-9999.", Assert.Single(validationProblem.Errors["request"]));
    }
}

public class VehicleTypesControllerTests
{
    private readonly IVehicleTypeService _vehicleTypeService = Substitute.For<IVehicleTypeService>();
    private readonly VehicleTypesController _controller;

    public VehicleTypesControllerTests()
    {
        _controller = new VehicleTypesController(_vehicleTypeService);
    }

    [Fact]
    public async Task GetVehicleTypes_ReturnsEveryRuleWithItsTollFreeFlag()
    {
        var car = VehicleTypeRule.Create(VehicleType.Car, isTollFree: false);
        var motorbike = VehicleTypeRule.Create(VehicleType.Motorbike, isTollFree: true);
        _vehicleTypeService.GetVehicleTypeRulesAsync(Arg.Any<CancellationToken>()).Returns([car, motorbike]);

        var result = await _controller.GetVehicleTypes(CancellationToken.None);

        var responses = Assert.IsType<VehicleTypeResponse[]>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(
            [ContractVehicleType.Car, ContractVehicleType.Motorbike],
            responses.Select(response => response.VehicleType));
        Assert.Equal([false, true], responses.Select(response => response.IsTollFree));
    }

}
