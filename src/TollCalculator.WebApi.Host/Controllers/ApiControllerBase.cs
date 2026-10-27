using Microsoft.AspNetCore.Mvc;
using TollCalculator.Application.Results;
using TollCalculator.WebApi.Host.Validators;

namespace TollCalculator.WebApi.Host.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult InvalidRequest(ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        var problemDetails = new ValidationProblemDetails(validationResult.Errors.ToDictionary())
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
        };

        return new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status400BadRequest };
    }

    protected ActionResult Failure<TValue>(ApplicationResult<TValue> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var statusCode = result.IsNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;

        var title = result.IsNotFound ? "Resource not found." : "The request could not be processed.";

        return ProblemResult(statusCode, title, result.ErrorMessage);
    }

    protected ActionResult InvalidCredentials()
    {
        return ProblemResult(
            StatusCodes.Status401Unauthorized,
            "Invalid client credentials.",
            "The client id and secret did not match a known client.");
    }

    private static ActionResult ProblemResult(int statusCode, string title, string? detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
