using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FleetGraph.Vehicles.Infrastructure.Persistence;

/// <summary>
/// Creates the schema and seeds demo data on startup. Runs as a hosted service (not in
/// Program.cs) so that <c>dotnet run -- schema export</c> never touches the database.
/// </summary>
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
                var db = scope.ServiceProvider.GetRequiredService<VehiclesDbContext>();

                await db.Database.EnsureCreatedAsync(cancellationToken);

                if (!await db.Vehicles.AnyAsync(cancellationToken))
                {
                    db.Vehicles.AddRange(VehiclesSeed.Create(clock.GetUtcNow()));
                    await db.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Seeded vehicles database.");
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
