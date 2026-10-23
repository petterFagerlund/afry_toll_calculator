using Microsoft.EntityFrameworkCore;
using TollCalculator.Infrastructure.Persistence;

namespace TollCalculator.Infrastructure.UnitTests.Repositories;

public sealed class RepositoryTestContext : IDisposable
{
    public RepositoryTestContext()
    {
        var options = new DbContextOptionsBuilder<TollDbContext>()
            .UseInMemoryDatabase($"toll-{Guid.CreateVersion7()}")
            .Options;

        Context = new TollDbContext(options);
        Context.Database.EnsureCreated();
    }

    public TollDbContext Context { get; }

    public void Dispose()
    {
        Context.Dispose();
    }
}
