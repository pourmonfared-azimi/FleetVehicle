using FleetGraph.Parts.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FleetGraph.Parts.Infrastructure.Persistence;

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
                var db = scope.ServiceProvider.GetRequiredService<PartsDbContext>();

                await db.Database.EnsureCreatedAsync(cancellationToken);

                if (!await db.Parts.AnyAsync(cancellationToken))
                {
                    var seed = PartsSeed.Create();
                    db.Parts.AddRange(seed.Select(s => s.Part));
                    await db.SaveChangesAsync(cancellationToken);

                    var now = clock.GetUtcNow();
                    db.StockLevels.AddRange(seed.Select(s => StockLevel.Create(s.Part.Id, s.OnHand, s.ReorderAt, now)));
                    await db.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Seeded parts database.");
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
