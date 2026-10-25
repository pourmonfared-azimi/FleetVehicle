using FleetGraph.Vehicles.Application.Abstractions;
using FleetGraph.Vehicles.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetGraph.Vehicles.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of the repository port. It is registered as scoped; Hot Chocolate
/// creates a dedicated service scope per resolver and per DataLoader batch, so parallel
/// resolvers never share a DbContext.
/// </summary>
internal sealed class VehicleRepository(VehiclesDbContext db) : IVehicleRepository
{
    public IQueryable<Vehicle> Query()
        => db.Vehicles.AsNoTracking().OrderBy(v => v.Id);

    public Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => db.Vehicles.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
        => await db.Vehicles
            .AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .ToListAsync(cancellationToken);

    public Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken)
        => db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Vin == vin, cancellationToken);

    public Task<bool> VinExistsAsync(string vin, CancellationToken cancellationToken)
        => db.Vehicles.AnyAsync(v => v.Vin == vin, cancellationToken);

    public void Add(Vehicle vehicle) => db.Vehicles.Add(vehicle);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => db.SaveChangesAsync(cancellationToken);
}
