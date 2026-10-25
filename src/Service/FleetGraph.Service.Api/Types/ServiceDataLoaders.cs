using FleetGraph.Service.Application.Queries;

namespace FleetGraph.Service.Api.Types;

/// <summary>
/// Source generated DataLoaders. Each one turns N resolver calls into one batched query,
/// which is what keeps nested federated queries free of N+1 problems.
/// </summary>
public static class ServiceDataLoaders
{
    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, ServiceOrderNode>> GetServiceOrderByIdAsync(
        IReadOnlyList<int> ids,
        ServiceReadService service,
        CancellationToken cancellationToken)
        => (await service.GetServiceOrdersByIdsAsync(ids, cancellationToken))
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToNode());

    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, ServiceOrderNode[]>> GetServiceOrdersByVehicleIdAsync(
        IReadOnlyList<int> vehicleIds,
        ServiceReadService service,
        CancellationToken cancellationToken)
        => (await service.GetServiceOrdersByVehicleIdsAsync(vehicleIds, cancellationToken))
            .ToDictionary(kv => kv.Key, kv => kv.Value.Select(o => o.ToNode()).ToArray());

    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, ServiceOrderLineNode[]>> GetLinesByServiceOrderIdAsync(
        IReadOnlyList<int> serviceOrderIds,
        ServiceReadService service,
        CancellationToken cancellationToken)
        => (await service.GetLinesByServiceOrderIdsAsync(serviceOrderIds, cancellationToken))
            .ToDictionary(kv => kv.Key, kv => kv.Value.Select(l => l.ToNode()).ToArray());

    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, WorkshopNode>> GetWorkshopByIdAsync(
        IReadOnlyList<int> ids,
        ServiceReadService service,
        CancellationToken cancellationToken)
        => (await service.GetWorkshopsByIdsAsync(ids, cancellationToken))
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToNode());
}
