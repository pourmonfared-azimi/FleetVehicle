using FleetGraph.Parts.Application.Abstractions;
using FleetGraph.Parts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class PartsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPartsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "InMemory";

        services.AddDbContext<PartsDbContext>(options =>
        {
            if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(configuration.GetConnectionString("Parts")
                    ?? throw new InvalidOperationException("ConnectionStrings:Parts is missing."));
            }
            else
            {
                options.UseInMemoryDatabase(configuration["Database:Name"] ?? "parts");
            }
        });

        services.AddScoped<IPartRepository, PartRepository>();
        services.AddScoped<IStockLevelRepository, StockLevelRepository>();
        services.AddHostedService<DatabaseInitializer>();
        return services;
    }
}
