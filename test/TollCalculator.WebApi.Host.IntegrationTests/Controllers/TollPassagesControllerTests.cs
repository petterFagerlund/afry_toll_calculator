using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TollCalculator.Application.Results;
using TollCalculator.Application.Services;
using TollCalculator.Domain.Models;
using TollCalculator.WebApi.Host.Controllers.V1;
using TollCalculator.WebApi.Host.Mappers.Implementations;
using TollCalculator.WebApi.Host.Validators.Implementation;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Controllers;

public class TollPassagesControllerTests
{
    private const string RegistrationNumber = "ABC123";

    private static readonly DateOnly Date = new(2026, 3, 16);
    private static readonly DateTimeOffset PassedAt = new(2026, 3, 16, 7, 15, 0, TimeSpan.FromHours(1));

    private readonly ITollPassageService _tollPassageService = Substitute.For<ITollPassageService>();
    private readonly ITollService _tollService = Substitute.For<ITollService>();
    private readonly TollPassagesController _controller;

    public TollPassagesControllerTests()
    {
        _controller = new TollPassagesController(
            _tollPassageService,
            _tollService,
            new TollPassageMapper(),
            new DailyTollMapper(),
            new RecordTollPassageRequestValidator(),
            new DailyTollQueryValidator());
    }

    [Fact]
    public async Task RecordTollPassage_ReturnsCreatedWithTheNewPassageId()
    {
        var passageId = Guid.CreateVersion7();
        _tollPassageService
            .RecordTollPassageAsync(RegistrationNumber, PassedAt, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<Guid>.Success(passageId));

        var result = await _controller.RecordTollPassage(NewRequest(), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal(passageId, Assert.IsType<RecordTollPassageResponse>(created.Value).TollPassageId);
    }

    [Fact]
    public async Task RecordTollPassage_PointsTheLocationAtTheDailyTollForThatDay()
    {
        _tollPassageService
            .RecordTollPassageAsync(RegistrationNumber, PassedAt, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<Guid>.Success(Guid.CreateVersion7()));

        var result = await _controller.RecordTollPassage(NewRequest(), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TollPassagesController.GetDailyToll), created.ActionName);
        Assert.Equal(RegistrationNumber, created.RouteValues!["registrationNumber"]);
        Assert.Equal(Date, created.RouteValues["date"]);
    }

    [Fact]
    public async Task RecordTollPassage_ReturnsBadRequestWhenTheRequestFailsValidation()
    {
        var result = await _controller.RecordTollPassage(new RecordTollPassageRequest(), CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.IsType<ValidationProblemDetails>(problem.Value);

        await _tollPassageService.DidNotReceiveWithAnyArgs()
            .RecordTollPassageAsync(default!, default, default);
    }

    [Fact]
    public async Task RecordTollPassage_ReturnsBadRequestWhenTheServiceRejectsTheInput()
    {
        _tollPassageService
            .RecordTollPassageAsync(RegistrationNumber, PassedAt, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<Guid>.Invalid("A registration number is required."));

        var result = await _controller.RecordTollPassage(NewRequest(), CancellationToken.None);

        AssertProblem(result.Result, StatusCodes.Status400BadRequest, "A registration number is required.");
    }

    [Fact]
    public async Task RecordTollPassage_ReturnsNotFoundForAnUnknownVehicle()
    {
        _tollPassageService
            .RecordTollPassageAsync(RegistrationNumber, PassedAt, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<Guid>.NotFound("No vehicle is registered with the number 'ABC123'."));

        var result = await _controller.RecordTollPassage(NewRequest(), CancellationToken.None);

        AssertProblem(result.Result, StatusCodes.Status404NotFound, "No vehicle is registered with the number 'ABC123'.");
    }

    [Fact]
    public async Task GetDailyToll_ReturnsTheDayTotalAndItsChargedPassages()
    {
        var dailyToll = new DailyToll(18, [new Domain.Models.ChargedTollPassage(PassedAt, 18)]);
        _tollService
            .GetDailyTollAsync(RegistrationNumber, Date, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<DailyToll>.Success(dailyToll));

        var result = await _controller.GetDailyToll(RegistrationNumber, Date, CancellationToken.None);

        var response = Assert.IsType<DailyTollResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(RegistrationNumber, response.RegistrationNumber);
        Assert.Equal(Date, response.Date);
        Assert.Equal(18, response.TotalFee);
        Assert.Equal(18, Assert.Single(response.ChargedPassages).Amount);
    }

    [Fact]
    public async Task GetDailyToll_ReturnsBadRequestWhenTheDateIsMissing()
    {
        var result = await _controller.GetDailyToll(RegistrationNumber, null, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.IsType<ValidationProblemDetails>(problem.Value);

        await _tollService.DidNotReceiveWithAnyArgs().GetDailyTollAsync(default!, default, default);
    }

    [Fact]
    public async Task GetDailyToll_ReturnsNotFoundForAnUnknownVehicle()
    {
        _tollService
            .GetDailyTollAsync(RegistrationNumber, Date, Arg.Any<CancellationToken>())
            .Returns(ApplicationResult<DailyToll>.NotFound("No vehicle is registered with the number 'ABC123'."));

        var result = await _controller.GetDailyToll(RegistrationNumber, Date, CancellationToken.None);

        AssertProblem(result.Result, StatusCodes.Status404NotFound, "No vehicle is registered with the number 'ABC123'.");
    }

    private static RecordTollPassageRequest NewRequest()
    {
        return new RecordTollPassageRequest { RegistrationNumber = RegistrationNumber, PassedAt = PassedAt };
    }

    private static void AssertProblem(IActionResult? result, int expectedStatus, string expectedDetail)
    {
        var problem = Assert.IsType<ObjectResult>(result);

        Assert.Equal(expectedStatus, problem.StatusCode);
        Assert.Equal(expectedDetail, Assert.IsType<ProblemDetails>(problem.Value).Detail);
    }
}
