using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TollCalculator.Infrastructure.Persistence;

namespace TollCalculator.Infrastructure.Registrations;

public static class ServiceProviderExtensions
{
    public static async Task<int> MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);

        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TollDbContext>();

        var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        var pendingCount = pending.Count();

        if (pendingCount == 0)
        {
            return 0;
        }

        await context.Database.MigrateAsync(cancellationToken);

        return pendingCount;
    }
}
