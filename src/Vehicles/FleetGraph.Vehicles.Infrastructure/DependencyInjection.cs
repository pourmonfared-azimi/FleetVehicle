using FleetGraph.Vehicles.Application.Abstractions;
using FleetGraph.Vehicles.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class VehiclesInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// <c>Database:Provider</c> = <c>Postgres</c> (Docker) or <c>InMemory</c> (default, local and tests).
    /// </summary>
    public static IServiceCollection AddVehiclesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "InMemory";

        services.AddDbContext<VehiclesDbContext>(options =>
        {
            if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(configuration.GetConnectionString("Vehicles")
                    ?? throw new InvalidOperationException("ConnectionStrings:Vehicles is missing."));
            }
            else
            {
                options.UseInMemoryDatabase(configuration["Database:Name"] ?? "vehicles");
            }
        });

        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddHostedService<DatabaseInitializer>();
        return services;
    }
}
