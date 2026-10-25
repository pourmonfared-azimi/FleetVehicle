using FleetGraph.Service.Application.Commands;
using FleetGraph.Service.Application.Queries;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddServiceApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<CreateServiceOrderHandler>();
        services.AddScoped<UpdateServiceOrderStatusHandler>();
        services.AddScoped<ServiceReadService>();
        return services;
    }
}
