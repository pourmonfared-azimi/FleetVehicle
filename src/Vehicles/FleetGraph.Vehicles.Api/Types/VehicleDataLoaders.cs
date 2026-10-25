using FleetGraph.Vehicles.Application.Queries;

namespace FleetGraph.Vehicles.Api.Types;

public static class VehicleDataLoaders
{
    /// <summary>
    /// Batches entity lookups. When the gateway resolves many vehicles at once (e.g. a list of
    /// service orders that each reference a vehicle) this becomes one SQL query.
    /// </summary>
    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, VehicleNode>> GetVehicleByIdAsync(
        IReadOnlyList<int> ids,
        VehicleReadService vehicles,
        CancellationToken cancellationToken)
    {
        var found = await vehicles.GetByIdsAsync(ids, cancellationToken);
        return found.ToDictionary(kv => kv.Key, kv => kv.Value.ToNode());
    }
}
