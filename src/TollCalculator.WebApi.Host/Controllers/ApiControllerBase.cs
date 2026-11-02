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
    protected ActionResult MapValidationProblem(ModelStateDictionary modelState)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        return ValidationProblem(new ValidationProblemDetails(modelState)
        {
            Status = StatusCodes.Status400BadRequest,
        });
    }

    protected ActionResult MapErrorResponse<TValue>(ApplicationResult<TValue> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        switch (result.Status)
        {
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
