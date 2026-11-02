using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TollCalculator.Application.Results;
using TollCalculator.WebApi.Host.Controllers;

namespace TollCalculator.WebApi.Host.IntegrationTests.Controllers;

public class ApiControllerBaseTests
{
    private readonly TestController _controller = new();

    [Fact]
    public void MapValidationProblem_ModelState_ReturnsEveryErrorKeyedByMember()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("registrationNumber", "A registration number is required.");
        modelState.AddModelError("date", "A date in the format yyyy-MM-dd is required.");

        var problem = ValidationProblemOf(_controller.MapValidationProblem(modelState));

        Assert.Equal("A registration number is required.", Assert.Single(problem.Errors["registrationNumber"]));
        Assert.Equal(
            "A date in the format yyyy-MM-dd is required.",
            Assert.Single(problem.Errors["date"]));
    }

    [Fact]
    public void MapValidationProblem_Message_KeysTheErrorOnTheRequest()
    {
        var problem = ValidationProblemOf(_controller.MapValidationProblem("'Hovercraft' is not a known vehicle type."));

        Assert.Equal("'Hovercraft' is not a known vehicle type.", Assert.Single(problem.Errors["request"]));
    }

    [Fact]
    public void MapValidationProblem_NullMessage_FallsBackToAPlaceholder()
    {
        var problem = ValidationProblemOf(_controller.MapValidationProblem(message: null));

        Assert.Equal("Unknown", Assert.Single(problem.Errors["request"]));
    }

    [Fact]
    public void MapErrorResponse_ValidationError_ReturnsBadRequest()
    {
        var result = _controller.MapErrorResponse(ApplicationResult<int>.Invalid("The year 0 is outside the supported range."));

        var problem = ValidationProblemOf(result);
        Assert.Equal("The year 0 is outside the supported range.", Assert.Single(problem.Errors["request"]));
    }

    [Fact]
    public void MapErrorResponse_NotFound_ReturnsNotFound()
    {
        var result = _controller.MapErrorResponse(ApplicationResult<int>.NotFound("No vehicle is registered with 'ABC123'."));

        var problem = ProblemOf(result, StatusCodes.Status404NotFound);
        Assert.Equal("No vehicle is registered with 'ABC123'.", problem.Detail);
    }

    [Fact]
    public void MapErrorResponse_Conflict_ReturnsConflict()
    {
        var result = _controller.MapErrorResponse(ApplicationResult<int>.Conflict("The passage is already recorded."));

        var problem = ProblemOf(result, StatusCodes.Status409Conflict);
        Assert.Equal("The passage is already recorded.", problem.Detail);
    }

    [Fact]
    public void MapErrorResponse_Error_ReturnsInternalServerError()
    {
        var result = _controller.MapErrorResponse(ApplicationResult<int>.Error("The toll schedule could not be read."));

        var problem = ProblemOf(result, StatusCodes.Status500InternalServerError);
        Assert.Equal("The toll schedule could not be read.", problem.Detail);
    }

    [Fact]
    public void MapErrorResponse_SuccessfulResult_Throws()
    {
        Assert.Throws<ArgumentException>(() => _controller.MapErrorResponse(ApplicationResult<int>.Success(1)));
    }

    [Fact]
    public void MapErrorResponse_NullResult_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _controller.MapErrorResponse<int>(null!));
    }

    private static ValidationProblemDetails ValidationProblemOf(ActionResult result)
    {
        var objectResult = Assert.IsType<BadRequestObjectResult>(result);

        return Assert.IsType<ValidationProblemDetails>(objectResult.Value);
    }

    private static ProblemDetails ProblemOf(ActionResult result, int expectedStatusCode)
    {
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);

        return Assert.IsType<ProblemDetails>(objectResult.Value);
    }

    private sealed class TestController : ApiControllerBase
    {
        public new ActionResult MapValidationProblem(ModelStateDictionary modelState)
        {
            return base.MapValidationProblem(modelState);
        }

        public new ActionResult MapValidationProblem(string? message)
        {
            return base.MapValidationProblem(message);
        }

        public new ActionResult MapErrorResponse<TValue>(ApplicationResult<TValue> result)
        {
            return base.MapErrorResponse(result);
        }
    }
}
