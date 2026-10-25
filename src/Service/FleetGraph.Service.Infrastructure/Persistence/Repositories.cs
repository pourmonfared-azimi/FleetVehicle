using FleetGraph.Service.Application.Abstractions;
using FleetGraph.Service.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetGraph.Service.Infrastructure.Persistence;

internal sealed class ServiceOrderRepository(ServiceDbContext db) : IServiceOrderRepository
{
    public IQueryable<ServiceOrder> Query()
        => db.ServiceOrders.AsNoTracking().OrderBy(o => o.Id);

    public Task<ServiceOrder?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => db.ServiceOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ServiceOrder>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
        => await db.ServiceOrders
            .AsNoTracking()
            .Where(o => ids.Contains(o.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ServiceOrder>> GetByVehicleIdsAsync(
        IReadOnlyCollection<int> vehicleIds,
        CancellationToken cancellationToken)
        => await db.ServiceOrders
            .AsNoTracking()
            .Where(o => vehicleIds.Contains(o.VehicleId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ServiceOrderLine>> GetLinesByServiceOrderIdsAsync(
        IReadOnlyCollection<int> serviceOrderIds,
        CancellationToken cancellationToken)
        => await db.ServiceOrderLines
            .AsNoTracking()
            .Where(l => serviceOrderIds.Contains(l.ServiceOrderId))
            .ToListAsync(cancellationToken);

    public void Add(ServiceOrder serviceOrder) => db.ServiceOrders.Add(serviceOrder);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => db.SaveChangesAsync(cancellationToken);
}

internal sealed class WorkshopRepository(ServiceDbContext db) : IWorkshopRepository
{
    public IQueryable<Workshop> Query()
        => db.Workshops.AsNoTracking().OrderBy(w => w.Id);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
        => db.Workshops.AnyAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Workshop>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
        => await db.Workshops
            .AsNoTracking()
            .Where(w => ids.Contains(w.Id))
            .ToListAsync(cancellationToken);
}
