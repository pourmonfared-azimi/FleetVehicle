using FleetGraph.Vehicles.Application.Queries;

namespace FleetGraph.Vehicles.Api.Types;

[QueryType]
public static partial class VehicleQueries
{
    [UsePaging]
    [UseFiltering(typeof(VehicleFilterInputType))]
    [UseSorting(typeof(VehicleSortInputType))]
    [GraphQLDescription("Vehicles with cursor pagination, filtering and sorting.")]
    public static IQueryable<VehicleNode> GetVehicles(VehicleReadService vehicles)
        => vehicles.Search().ToNodes();

    /// <summary>
    /// Public lookup: clients can call it, and the Fusion gateway uses it to resolve a
    /// <c>Vehicle</c> referenced from another subgraph (the key is <c>id</c>).
    /// </summary>
    [Lookup]
    public static async Task<VehicleNode?> GetVehicleByIdAsync(
        int id,
        IVehicleByIdDataLoader vehicleById,
        CancellationToken cancellationToken)
        => await vehicleById.LoadAsync(id, cancellationToken);

    public static async Task<VehicleNode?> GetVehicleByVinAsync(
        string vin,
        VehicleReadService vehicles,
        CancellationToken cancellationToken)
        => (await vehicles.GetByVinAsync(vin, cancellationToken))?.ToNode();
}
