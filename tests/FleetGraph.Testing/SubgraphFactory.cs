using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FleetGraph.Testing;

/// <summary>
/// Hosts one subgraph in memory with its own isolated in-memory database (seeded on start).
/// </summary>
/// <typeparam name="TMarker">Any public type from the subgraph's Api assembly.</typeparam>
public class SubgraphFactory<TMarker> : WebApplicationFactory<TMarker>
    where TMarker : class
{
    private readonly string _databaseName = $"test-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Database:Provider", "InMemory");
        builder.UseSetting("Database:Name", _databaseName);
        builder.UseSetting("ConnectionStrings:Redis", string.Empty);
        builder.UseSetting("OTEL_EXPORTER_OTLP_ENDPOINT", string.Empty);
    }
}
