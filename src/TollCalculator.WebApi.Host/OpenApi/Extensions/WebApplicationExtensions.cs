using Scalar.AspNetCore;
using TollCalculator.WebApi.Host.OpenApi.Transformers;

namespace TollCalculator.WebApi.Host.OpenApi.Extensions;

internal static class WebApplicationExtensions
{
    public const string ScalarRoute = "/scalar";

    public static WebApplication MapApiReference(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapOpenApi();

        app.MapScalarApiReference(ScalarRoute, options =>
        {
            options.WithTitle(ApiVersionInfoDocumentTransformer.Title);

            foreach (var documentName in ApiDocuments.All)
            {
                options.AddDocument(documentName);
            }
        });

        return app;
    }
}
