using Xunit;

namespace TollCalculator.WebApi.Host.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class TollApiCollection : ICollectionFixture<TollApiFixture>
{
    public const string Name = "Toll API";
}
