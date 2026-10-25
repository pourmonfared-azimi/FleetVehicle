using FleetGraph.Service.Domain;

namespace FleetGraph.Service.Application.Abstractions;

public interface IServiceOrderRepository
{
    IQueryable<ServiceOrder> Query();

    /// <summary>Loads the aggregate (with lines) for modification.</summary>
    Task<ServiceOrder?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ServiceOrder>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ServiceOrder>> GetByVehicleIdsAsync(
        IReadOnlyCollection<int> vehicleIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ServiceOrderLine>> GetLinesByServiceOrderIdsAsync(
        IReadOnlyCollection<int> serviceOrderIds,
        CancellationToken cancellationToken);

    void Add(ServiceOrder serviceOrder);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
