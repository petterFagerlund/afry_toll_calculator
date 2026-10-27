using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TollCalculator.Application.Results;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Controllers.V1;
using TollCalculator.WebApi.Host.Mappers;
using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Controllers;

public class TollControllerTests
{
    private static readonly DateTimeOffset Morning = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));

    private readonly ITollService _tollService = Substitute.For<ITollService>();
    private readonly TollController _controller;

    public TollControllerTests()
    {
        _controller = new TollController(_tollService);
    }

    [Fact]
    public async Task Calculate_ReturnsTheTotalAndChargedPassages()
    {
        var dailyToll = new DailyToll(18, [new Domain.Models.ChargedTollPassage(Morning, 18)]);
        _tollService
            .CalculateAsync("Car", Arg.Is<DateTimeOffset[]>(timestamps => timestamps.Length == 1),
                Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<DailyToll>.Success(dailyToll));

        var result = await _controller.Calculate(NewRequest("Car"), CancellationToken.None);

        var response = Assert.IsType<CalculateTollResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("Car", response.VehicleType);
        Assert.Equal(18, response.TotalFee);
        Assert.Equal(18, Assert.Single(response.ChargedPassages).Amount);
    }

    [Fact]
    public async Task Calculate_ReturnsBadRequestWhenTheRequestFailsValidation()
    {
        var result = await _controller.Calculate(new CalculateTollRequest(), CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.IsType<ValidationProblemDetails>(problem.Value);

        await _tollService.DidNotReceiveWithAnyArgs().CalculateAsync(default!, default!, default);
    }

    [Fact]
    public async Task Calculate_ReturnsBadRequestForAVehicleTypeTheServiceDoesNotKnow()
    {
        _tollService
            .CalculateAsync("Hovercraft", Arg.Any<DateTimeOffset[]>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<DailyToll>.Invalid("'Hovercraft' is not a known vehicle type."));

        var result = await _controller.Calculate(NewRequest("Hovercraft"), CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.Equal("'Hovercraft' is not a known vehicle type.",
            Assert.IsType<ProblemDetails>(problem.Value).Detail);
    }

    [Fact]
    public async Task Calculate_ReturnsZeroForATollFreeVehicle()
    {
        _tollService
            .CalculateAsync("Motorbike", Arg.Any<DateTimeOffset[]>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<DailyToll>.Success(DailyToll.None()));

        var result = await _controller.Calculate(NewRequest("Motorbike"), CancellationToken.None);

        var response = Assert.IsType<CalculateTollResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(0, response.TotalFee);
        Assert.Empty(response.ChargedPassages);
    }

    private static CalculateTollRequest NewRequest(string vehicleType)
    {
        return new CalculateTollRequest { VehicleType = vehicleType, Timestamps = [Morning] };
    }
}
