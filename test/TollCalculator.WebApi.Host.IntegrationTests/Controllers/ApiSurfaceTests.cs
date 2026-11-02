using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using TollCalculator.WebApi.Host.Controllers.V1;
using TollCalculator.WebApi.Host.Registrations;
using TollCalculator.WebApi.Models.V1.Responses;

namespace TollCalculator.WebApi.Host.IntegrationTests.Controllers;

public class ApiSurfaceTests
{
    private const string RecordTollPassageRoute = "api/v1/toll-passages";
    private const string DailyTollRoute = "api/v1/toll-calculations";

    private static readonly ApiDescription[] Descriptions = DescribeApi();

    [Theory]
    [InlineData("POST", RecordTollPassageRoute)]
    [InlineData("POST", DailyTollRoute)]
    public void TheApiDescription_IncludesTheRoute(string httpMethod, string relativePath)
    {
        Assert.Single(Descriptions, description =>
            description.HttpMethod == httpMethod && description.RelativePath == relativePath);
    }

    [Theory]
    [InlineData(201, typeof(RecordTollPassagesResponse))]
    [InlineData(400, null)]
    [InlineData(409, null)]
    public void RecordTollPassages_DeclaresItsResponseType(int statusCode, Type? responseType)
    {
        AssertDeclaresResponse("POST", RecordTollPassageRoute, statusCode, responseType);
    }

    [Fact]
    public void RecordTollPassages_NoLongerDeclaresNotFound()
    {
        var description = Descriptions.Single(candidate =>
            candidate.HttpMethod == "POST" && candidate.RelativePath == RecordTollPassageRoute);

        Assert.DoesNotContain(description.SupportedResponseTypes, response => response.StatusCode == 404);
    }

    [Fact]
    public void CalculateDailyToll_NoLongerDeclaresNotFound()
    {
        var description = Descriptions.Single(candidate =>
            candidate.HttpMethod == "POST" && candidate.RelativePath == DailyTollRoute);

        Assert.DoesNotContain(description.SupportedResponseTypes, response => response.StatusCode == 404);
    }

    [Theory]
    [InlineData(200, typeof(DailyTollResponse))]
    [InlineData(400, null)]
    public void CalculateDailyToll_DeclaresItsResponseType(int statusCode, Type? responseType)
    {
        AssertDeclaresResponse("POST", DailyTollRoute, statusCode, responseType);
    }

    private static void AssertDeclaresResponse(string httpMethod, string relativePath, int statusCode, Type? responseType)
    {
        var description = Descriptions.Single(candidate =>
            candidate.HttpMethod == httpMethod && candidate.RelativePath == relativePath);

        var response = Assert.Single(description.SupportedResponseTypes, candidate =>
            candidate.StatusCode == statusCode);

        if (responseType is not null)
        {
            Assert.Equal(responseType, response.Type);
        }
    }

    private static ApiDescription[] DescribeApi()
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.Services.AddControllers().AddApplicationPart(typeof(TollController).Assembly);
        builder.Services.AddVersionedApi();

        using var application = builder.Build();

        var groups = application.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

        return groups.ApiDescriptionGroups.Items.SelectMany(group => group.Items).ToArray();
    }
}
