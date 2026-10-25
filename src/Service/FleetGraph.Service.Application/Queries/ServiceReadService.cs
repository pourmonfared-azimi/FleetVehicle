using FleetGraph.Service.Application.Abstractions;
using FleetGraph.Service.Domain;

namespace FleetGraph.Service.Application.Queries;

/// <summary>Read side use cases (and the batch reads that back the DataLoaders).</summary>
public sealed class ServiceReadService(
    IServiceOrderRepository serviceOrders,
    IWorkshopRepository workshops)
{
    public IQueryable<ServiceOrder> SearchServiceOrders() => serviceOrders.Query();

    public IQueryable<Workshop> SearchWorkshops() => workshops.Query();

    public async Task<IReadOnlyDictionary<int, ServiceOrder>> GetServiceOrdersByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
        => (await serviceOrders.GetByIdsAsync(ids, cancellationToken)).ToDictionary(o => o.Id);

    public async Task<IReadOnlyDictionary<int, ServiceOrder[]>> GetServiceOrdersByVehicleIdsAsync(
        IReadOnlyCollection<int> vehicleIds,
        CancellationToken cancellationToken)
        => (await serviceOrders.GetByVehicleIdsAsync(vehicleIds, cancellationToken))
            .GroupBy(o => o.VehicleId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(o => o.ScheduledAt).ToArray());

    public async Task<IReadOnlyDictionary<int, ServiceOrderLine[]>> GetLinesByServiceOrderIdsAsync(
        IReadOnlyCollection<int> serviceOrderIds,
        CancellationToken cancellationToken)
        => (await serviceOrders.GetLinesByServiceOrderIdsAsync(serviceOrderIds, cancellationToken))
            .GroupBy(l => l.ServiceOrderId)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.Id).ToArray());

    public async Task<IReadOnlyDictionary<int, Workshop>> GetWorkshopsByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
        => (await workshops.GetByIdsAsync(ids, cancellationToken)).ToDictionary(w => w.Id);
}
