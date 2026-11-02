using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TollCalculator.Application.Results;

namespace TollCalculator.WebApi.Host.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
public abstract class ApiControllerBase : ControllerBase
{
    private const string RequestErrorKey = "request";
    private const string UnknownErrorMessage = "Unknown";

    protected ActionResult MapValidationProblem(ModelStateDictionary modelState)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        return ValidationProblem(new ValidationProblemDetails(modelState)
        {
            Status = StatusCodes.Status400BadRequest,
        });
    }

    protected ActionResult MapValidationProblem(string? message)
    {
        return ValidationProblem(new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Errors = new Dictionary<string, string[]>
            {
                { RequestErrorKey, [message ?? UnknownErrorMessage] },
            },
        });
    }

    protected ActionResult MapErrorResponse<TValue>(ApplicationResult<TValue> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        switch (result.Status)
        {
            case ApplicationResultStatus.ValidationError:
                return MapValidationProblem(result.ErrorMessage);
            case ApplicationResultStatus.NotFound:
                return ProblemResult(StatusCodes.Status404NotFound, "Resource not found.", result.ErrorMessage);
            case ApplicationResultStatus.Conflict:
                return ProblemResult(
                    StatusCodes.Status409Conflict,
                    "The request conflicts with the current state.",
                    result.ErrorMessage);
            case ApplicationResultStatus.Error:
                return ProblemResult(
                    StatusCodes.Status500InternalServerError,
                    "The request could not be processed.",
                    result.ErrorMessage);
            default:
                throw new ArgumentException(
                    $"An application result with the status {result.Status} is not an error.",
                    nameof(result));
        }
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
