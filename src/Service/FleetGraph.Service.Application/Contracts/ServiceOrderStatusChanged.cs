using FleetGraph.Service.Domain;

namespace FleetGraph.Service.Application.Contracts;

/// <summary>
/// Integration event raised when a service order changes status. It is a plain serializable
/// record (it travels through Redis when the subgraph is scaled out).
/// </summary>
public sealed record ServiceOrderStatusChanged(
    int ServiceOrderId,
    int VehicleId,
    ServiceOrderStatus PreviousStatus,
    ServiceOrderStatus NewStatus,
    DateTimeOffset ChangedAt);
