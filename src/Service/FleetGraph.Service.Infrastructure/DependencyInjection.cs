using FleetGraph.Service.Application.Abstractions;
using FleetGraph.Service.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "InMemory";

        services.AddDbContext<ServiceDbContext>(options =>
        {
            if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(configuration.GetConnectionString("Service")
                    ?? throw new InvalidOperationException("ConnectionStrings:Service is missing."));
            }
            else
            {
                options.UseInMemoryDatabase(configuration["Database:Name"] ?? "service");
            }
        });

        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
        services.AddScoped<IWorkshopRepository, WorkshopRepository>();
        services.AddHostedService<DatabaseInitializer>();
        return services;
    }
}
