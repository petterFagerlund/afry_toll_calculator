using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TollCalculator.WebApi.Host.OpenApi.Transformers;

internal sealed class ApiVersionInfoDocumentTransformer : IOpenApiDocumentTransformer
{
    public const string Title = "Toll Calculator API";

    private const string Summary =
        "Records toll passages and calculates what a vehicle owes for a day, with the toll schedule, "
        + "holidays and vehicle types it is based on.";

    private readonly string _documentName;

    public ApiVersionInfoDocumentTransformer(string documentName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentName);

        _documentName = documentName;
    }

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        document.Info ??= new OpenApiInfo();
        document.Info.Title = Title;
        document.Info.Version = _documentName;
        document.Info.Description = Summary;

        return Task.CompletedTask;
    }
}
