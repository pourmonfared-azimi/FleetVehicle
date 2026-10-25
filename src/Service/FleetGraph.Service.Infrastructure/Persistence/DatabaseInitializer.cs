using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FleetGraph.Service.Infrastructure.Persistence;

/// <summary>Creates the schema and seeds demo data (hosted service, so schema export skips it).</summary>
internal sealed class DatabaseInitializer(
    IServiceProvider services,
    TimeProvider clock,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();

                await db.Database.EnsureCreatedAsync(cancellationToken);

                if (!await db.Workshops.AnyAsync(cancellationToken))
                {
                    db.Workshops.AddRange(ServiceSeed.Workshops());
                    await db.SaveChangesAsync(cancellationToken);

                    db.ServiceOrders.AddRange(ServiceSeed.ServiceOrders(clock.GetUtcNow()));
                    await db.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Seeded service database.");
                }

                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{Max}), retrying...", attempt, maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
