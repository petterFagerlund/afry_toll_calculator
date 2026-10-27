using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NSubstitute;
using TollCalculator.WebApi.Host.Auth;
using TollCalculator.WebApi.Host.Controllers.V1;
using TollCalculator.WebApi.Host.Validators.Implementations;
using TollCalculator.WebApi.Models.V1.Requests;
using TollCalculator.WebApi.Models.V1.Responses;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Controllers;

public class AuthControllerTests
{
    private const string ClientId = "toll-client";
    private const string ClientSecret = "a-client-secret-long-enough";

    private static readonly DateTimeOffset ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60);

    private readonly IJwtTokenIssuer _tokenIssuer = Substitute.For<IJwtTokenIssuer>();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_tokenIssuer);
    }

    [Fact]
    public void IssueToken_IsAnonymousSoATokenIsNotNeededToGetAToken()
    {
        Assert.NotNull(IssueTokenMethod().GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void IssueToken_IsRateLimited()
    {
        var rateLimiting = IssueTokenMethod().GetCustomAttribute<EnableRateLimitingAttribute>();

        Assert.NotNull(rateLimiting);
        Assert.Equal(RateLimitPolicies.TokenIssuance, rateLimiting.PolicyName);
    }

    [Fact]
    public void IssueToken_ReturnsTheIssuedToken()
    {
        _tokenIssuer.Issue(ClientId, ClientSecret)
            .Returns(new IssuedToken("a.b.c", "Bearer", ExpiresAt));

        var result = _controller.IssueToken(new TokenRequest { ClientId = ClientId, ClientSecret = ClientSecret });

        var response = Assert.IsType<TokenResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("a.b.c", response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(ExpiresAt, response.ExpiresAt);
    }

    [Fact]
    public void IssueToken_ReturnsUnauthorizedForCredentialsTheIssuerRejects()
    {
        _tokenIssuer.Issue(ClientId, "wrong-secret").Returns((IssuedToken?)null);

        var result = _controller.IssueToken(new TokenRequest { ClientId = ClientId, ClientSecret = "wrong-secret" });

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.StatusCode);
        Assert.IsType<ProblemDetails>(problem.Value);
    }

    [Fact]
    public void IssueToken_ReturnsBadRequestWhenTheRequestFailsValidation()
    {
        var result = _controller.IssueToken(new TokenRequest());

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.IsType<ValidationProblemDetails>(problem.Value);

        _tokenIssuer.DidNotReceiveWithAnyArgs().Issue(default!, default!);
    }

    [Fact]
    public void IssueToken_DoesNotRepeatTheSecretBackInTheErrorResponse()
    {
        _tokenIssuer.Issue(ClientId, "super-secret").Returns((IssuedToken?)null);

        var result = _controller.IssueToken(new TokenRequest { ClientId = ClientId, ClientSecret = "super-secret" });

        var problem = Assert.IsType<ProblemDetails>(Assert.IsType<ObjectResult>(result.Result).Value);

        Assert.DoesNotContain("super-secret", $"{problem.Title} {problem.Detail}", StringComparison.Ordinal);
    }

    private static MethodInfo IssueTokenMethod()
    {
        return typeof(AuthController).GetMethod(nameof(AuthController.IssueToken))!;
    }
}
