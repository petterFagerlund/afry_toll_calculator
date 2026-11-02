using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TollCalculator.Application.Results;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Enums;
using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Controllers.V1;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.IntegrationTests.Controllers;

public class TollPassagesControllerTests
{
    private const string RegistrationNumber = "ABC123";

    private static readonly DateOnly Date = new(2026, 3, 16);
    private static readonly DateTimeOffset PassedAt = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));

    private readonly ITollPassageService _tollPassageService = Substitute.For<ITollPassageService>();
    private readonly ITollService _tollService = Substitute.For<ITollService>();
    private readonly TollController _controller;

    public TollPassagesControllerTests()
    {
        _controller = new TollController(_tollPassageService, _tollService, TimeProvider.System);
    }

    [Fact]
    public async Task RecordTollPassages_ReturnsCreatedWithTheNewPassageIds()
    {
        var passageIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };
        RecordingReturns(ApplicationResult<Guid[]>.Success(passageIds));

        var result = await _controller.RecordTollPassages(NewRequest(), CancellationToken.None);

        var created = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal(passageIds, Assert.IsType<RecordTollPassagesResponse>(created.Value).TollPassageIds);
    }

    [Fact]
    public async Task RecordTollPassages_PassesEveryPassageToTheServiceWithItsReportedType()
    {
        RecordingReturns(ApplicationResult<Guid[]>.Success([Guid.CreateVersion7(), Guid.CreateVersion7()]));

        await _controller.RecordTollPassages(NewRequest(), CancellationToken.None);

        await _tollPassageService.Received(1).RecordTollPassagesAsync(
            Arg.Is<TollPassage[]>(passages =>
                passages.Length == 2
                && passages[0].RegistrationNumber == RegistrationNumber
                && passages[0].VehicleType == VehicleType.Car
                && passages[1].VehicleType == VehicleType.Motorbike),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordTollPassages_ReturnsBadRequestWhenAPassageHasABlankVehicleType()
    {
        var request = new RecordTollPassagesRequest
        {
            Passages =
            [
                new TollPassageToRecord
                {
                    RegistrationNumber = RegistrationNumber,
                    VehicleType = "",
                    PassedAt = PassedAt,
                },
            ],
        };

        var result = await _controller.RecordTollPassages(request, CancellationToken.None);

        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.Contains(
            "passages[0].VehicleType",
            Assert.IsType<ValidationProblemDetails>(problem.Value).Errors.Keys);

        await _tollPassageService.DidNotReceiveWithAnyArgs()
            .RecordTollPassagesAsync(null!, CancellationToken.None);
    }

    [Fact]
    public async Task RecordTollPassages_ReturnsBadRequestWhenAPassageHasAnUnknownVehicleType()
    {
        var request = new RecordTollPassagesRequest
        {
            Passages =
            [
                new TollPassageToRecord
                {
                    RegistrationNumber = RegistrationNumber,
                    VehicleType = "Submarine",
                    PassedAt = PassedAt,
                },
            ],
        };

        var result = await _controller.RecordTollPassages(request, CancellationToken.None);

        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains(
            "passages[0].VehicleType",
            Assert.IsType<ValidationProblemDetails>(problem.Value).Errors.Keys);
    }

    [Fact]
    public async Task RecordTollPassages_ReturnsBadRequestWhenNoPassagesAreSupplied()
    {
        var result = await _controller.RecordTollPassages(
            new RecordTollPassagesRequest { Passages = [] }, CancellationToken.None);

        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.IsType<ValidationProblemDetails>(problem.Value);
    }

    [Fact]
    public async Task RecordTollPassages_ReturnsConflictWhenAPassageIsAlreadyRecorded()
    {
        RecordingReturns(ApplicationResult<Guid[]>.Conflict("One or more of the passages is already recorded."));

        var result = await _controller.RecordTollPassages(NewRequest(), CancellationToken.None);

        AssertProblem(
            result.Result,
            StatusCodes.Status409Conflict,
            "One or more of the passages is already recorded.");
    }

    [Fact]
    public async Task CalculateDailyToll_ReturnsTheDayTotalAndItsChargedPassages()
    {
        var dailyToll = new DailyToll(18, [new Domain.Models.ChargedTollPassage(PassedAt, 18)]);
        _tollService
            .GetDailyTollAsync(RegistrationNumber, Date, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<DailyToll>.Success(dailyToll));

        var result = await _controller.CalculateDailyToll(NewCalculationRequest(), CancellationToken.None);

        var response = Assert.IsType<DailyTollResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(RegistrationNumber, response.RegistrationNumber);
        Assert.Equal(Date, response.Date);
        Assert.Equal(18, response.TotalFee);
        Assert.Equal(18, Assert.Single(response.ChargedPassages).Amount);
    }

    [Fact]
    public async Task CalculateDailyToll_ReturnsZeroForARegistrationNumberWithNoPassages()
    {
        _tollService
            .GetDailyTollAsync(RegistrationNumber, Date, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<DailyToll>.Success(DailyToll.None()));

        var result = await _controller.CalculateDailyToll(NewCalculationRequest(), CancellationToken.None);

        var response = Assert.IsType<DailyTollResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(0, response.TotalFee);
        Assert.Empty(response.ChargedPassages);
    }

    private static CalculateDailyTollRequest NewCalculationRequest()
    {
        return new CalculateDailyTollRequest { RegistrationNumber = RegistrationNumber, Date = Date };
    }

    private void RecordingReturns(ApplicationResult<Guid[]> result)
    {
        _tollPassageService
            .RecordTollPassagesAsync(Arg.Any<TollPassage[]>(), Arg.Any<CancellationToken>())
            .Returns(result);
    }

    private static RecordTollPassagesRequest NewRequest()
    {
        return new RecordTollPassagesRequest
        {
            Passages =
            [
                new TollPassageToRecord
                {
                    RegistrationNumber = RegistrationNumber,
                    VehicleType = nameof(VehicleType.Car),
                    PassedAt = PassedAt,
                },
                new TollPassageToRecord
                {
                    RegistrationNumber = "XYZ789",
                    VehicleType = nameof(VehicleType.Motorbike),
                    PassedAt = PassedAt,
                },
            ],
        };
    }

    private static void AssertProblem(IActionResult? result, int expectedStatus, string expectedDetail)
    {
        var problem = Assert.IsType<ObjectResult>(result);

        Assert.Equal(expectedStatus, problem.StatusCode);
        Assert.Equal(expectedDetail, Assert.IsType<ProblemDetails>(problem.Value).Detail);
    }
}
