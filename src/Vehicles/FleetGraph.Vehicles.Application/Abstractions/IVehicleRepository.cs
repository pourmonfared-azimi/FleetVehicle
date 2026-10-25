using FleetGraph.Vehicles.Domain;

namespace FleetGraph.Vehicles.Application.Abstractions;

/// <summary>
/// Persistence port for the Vehicle aggregate. Implemented in the Infrastructure layer.
/// </summary>
public interface IVehicleRepository
{
    /// <summary>
    /// A read-only, composable query. Exposed as <see cref="IQueryable{T}"/> on purpose so the
    /// GraphQL layer can push filtering, sorting and paging down to the database.
    /// </summary>
    IQueryable<Vehicle> Query();

    Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Vehicle>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken);

    Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken);

    Task<bool> VinExistsAsync(string vin, CancellationToken cancellationToken);

    void Add(Vehicle vehicle);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
