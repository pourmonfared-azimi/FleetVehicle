using FleetGraph.Parts.Application.Commands;
using FleetGraph.Parts.Application.Queries;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class PartsApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddPartsApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<UpdateStockHandler>();
        services.AddScoped<PartReadService>();
        return services;
    }
}
