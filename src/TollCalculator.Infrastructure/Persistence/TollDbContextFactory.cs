using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TollCalculator.Infrastructure.Persistence;

internal sealed class TollDbContextFactory : IDesignTimeDbContextFactory<TollDbContext>
{
    private const string ConnectionStringVariable = "TOLLDB_CONNECTION";

    private const string DefaultConnectionString =
        "Server=localhost;Database=TollCalculator;Trusted_Connection=True;TrustServerCertificate=True";

    public TollDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DefaultConnectionString;
        }

        var options = new DbContextOptionsBuilder<TollDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new TollDbContext(options);
    }
}
