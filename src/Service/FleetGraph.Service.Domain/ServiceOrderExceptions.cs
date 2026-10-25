using FleetGraph.SharedKernel;

namespace FleetGraph.Service.Domain;

public sealed class InvalidServiceOrderStatusTransitionException(
    ServiceOrderStatus from,
    ServiceOrderStatus to)
    : DomainException(
        "INVALID_STATUS_TRANSITION",
        $"A service order cannot change status from {from} to {to}.")
{
    public ServiceOrderStatus From { get; } = from;

    public ServiceOrderStatus To { get; } = to;
}
