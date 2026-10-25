using FleetGraph.Vehicles.Application.Abstractions;
using FleetGraph.Vehicles.Domain;

namespace FleetGraph.Vehicles.Application.Queries;

/// <summary>Read side use cases for vehicles.</summary>
public sealed class VehicleReadService(IVehicleRepository vehicles)
{
    public IQueryable<Vehicle> Search() => vehicles.Query();

    public Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken)
        => vehicles.GetByVinAsync(vin.Trim().ToUpperInvariant(), cancellationToken);

    /// <summary>Batch read used by DataLoaders and by Fusion entity lookups.</summary>
    public async Task<IReadOnlyDictionary<int, Vehicle>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new Dictionary<int, Vehicle>();
        }

        var result = await vehicles.GetByIdsAsync(ids, cancellationToken);
        return result.ToDictionary(v => v.Id);
    }
}
