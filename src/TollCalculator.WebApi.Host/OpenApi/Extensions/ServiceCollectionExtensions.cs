using TollCalculator.WebApi.Host.OpenApi.Transformers;

namespace TollCalculator.WebApi.Host.OpenApi.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVersionedOpenApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        foreach (var documentName in ApiDocuments.All)
        {
            services.AddOpenApi(documentName, options =>
            {
                options.AddDocumentTransformer(new ApiVersionInfoDocumentTransformer(documentName));
                options.AddDocumentTransformer(new BearerSecuritySchemeDocumentTransformer());
            });
        }

        return services;
    }
}
