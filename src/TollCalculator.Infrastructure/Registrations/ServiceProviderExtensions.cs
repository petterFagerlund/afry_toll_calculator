using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TollCalculator.Infrastructure.Persistence;

namespace TollCalculator.Infrastructure.Registrations;

public static class ServiceProviderExtensions
{
    public static async Task<int> MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);

        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TollDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TollDbContext>>();

        try
        {
            var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingCount = pending.Count();

            if (pendingCount == 0)
            {
                logger.LogInformation("The toll database is up to date; no migrations were applied.");

                return 0;
            }

            await context.Database.MigrateAsync(cancellationToken);

            logger.LogInformation("Applied {MigrationCount} migrations to the toll database.", pendingCount);

            return pendingCount;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Migrating the toll database failed; the application cannot start.");

            throw new InvalidOperationException(
                "Migrating the toll database failed. Check that the TollDatabase connection string points at a " +
                "reachable SQL Server and that the account may create and alter objects.",
                exception);
        }
    }
}
