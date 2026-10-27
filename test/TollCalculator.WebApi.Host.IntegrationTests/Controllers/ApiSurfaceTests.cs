using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using TollCalculator.WebApi.Host.Controllers.V1;
using TollCalculator.WebApi.Host.Registrations;
using TollCalculator.WebApi.Models.V1.Responses;
using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Controllers;

public class ApiSurfaceTests
{
    private const string RecordTollPassageRoute = "api/v1/toll-passages";
    private const string DailyTollRoute = "api/v1/vehicles/{registrationNumber}/toll";

    private static readonly ApiDescription[] Descriptions = DescribeApi();

    [Theory]
    [InlineData("POST", RecordTollPassageRoute)]
    [InlineData("GET", DailyTollRoute)]
    public void TheApiDescription_IncludesTheRoute(string httpMethod, string relativePath)
    {
        Assert.Single(Descriptions, description =>
            description.HttpMethod == httpMethod && description.RelativePath == relativePath);
    }

    [Theory]
    [InlineData(201, typeof(RecordTollPassageResponse))]
    [InlineData(400, null)]
    [InlineData(404, null)]
    public void RecordTollPassage_DeclaresItsResponseType(int statusCode, Type? responseType)
    {
        AssertDeclaresResponse("POST", RecordTollPassageRoute, statusCode, responseType);
    }

    [Theory]
    [InlineData(200, typeof(DailyTollResponse))]
    [InlineData(400, null)]
    [InlineData(404, null)]
    public void GetDailyToll_DeclaresItsResponseType(int statusCode, Type? responseType)
    {
        AssertDeclaresResponse("GET", DailyTollRoute, statusCode, responseType);
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

        builder.Services.AddControllers().AddApplicationPart(typeof(TollPassagesController).Assembly);
        builder.Services.AddVersionedApi();

        using var application = builder.Build();

        var groups = application.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

        return groups.ApiDescriptionGroups.Items.SelectMany(group => group.Items).ToArray();
    }
}
