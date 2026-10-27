using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TollCalculator.WebApi.Host.Auth;
using TollCalculator.WebApi.Host.Validators;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.Controllers.V1;

[ApiVersion("1.0")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IJwtTokenIssuer _tokenIssuer;
    private readonly ITokenRequestValidator _tokenRequestValidator;

    public AuthController(IJwtTokenIssuer tokenIssuer, ITokenRequestValidator tokenRequestValidator)
    {
        ArgumentNullException.ThrowIfNull(tokenIssuer);
        ArgumentNullException.ThrowIfNull(tokenRequestValidator);

        _tokenIssuer = tokenIssuer;
        _tokenRequestValidator = tokenRequestValidator;
    }

    /// <summary>Exchanges client credentials for a bearer token that the other endpoints accept.</summary>
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.TokenIssuance)]
    [HttpPost("api/v{version:apiVersion}/auth/token")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public ActionResult<TokenResponse> IssueToken([FromBody] TokenRequest request)
    {
        var validationResult = _tokenRequestValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return InvalidRequest(validationResult);
        }

        var issued = _tokenIssuer.Issue(request.ClientId!, request.ClientSecret!);

        if (issued is null)
        {
            return InvalidCredentials();
        }

        return Ok(new TokenResponse
        {
            AccessToken = issued.AccessToken,
            TokenType = issued.TokenType,
            ExpiresAt = issued.ExpiresAt,
        });
    }
}
