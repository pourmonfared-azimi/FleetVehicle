using FleetGraph.Vehicles.Application.Commands;
using FleetGraph.Vehicles.Application.Queries;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class VehiclesApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddVehiclesApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<RegisterVehicleHandler>();
        services.AddScoped<ChangeVehicleStatusHandler>();
        services.AddScoped<RecordMileageHandler>();
        services.AddScoped<VehicleReadService>();
        return services;
    }
}
