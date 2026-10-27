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

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = result.IsNotFound ? "Resource not found." : "The request could not be processed.",
            Detail = result.ErrorMessage,
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
