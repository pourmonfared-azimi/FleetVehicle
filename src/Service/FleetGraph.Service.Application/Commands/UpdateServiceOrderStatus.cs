using FleetGraph.Service.Application.Abstractions;
using FleetGraph.Service.Application.Contracts;
using FleetGraph.Service.Domain;
using FleetGraph.SharedKernel;

namespace FleetGraph.Service.Application.Commands;

public sealed record UpdateServiceOrderStatusCommand(int ServiceOrderId, ServiceOrderStatus Status);

public sealed class UpdateServiceOrderStatusHandler(
    IServiceOrderRepository serviceOrders,
    IServiceOrderEventPublisher events,
    TimeProvider clock)
{
    public async Task<ServiceOrder> HandleAsync(
        UpdateServiceOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        var order = await serviceOrders.GetByIdAsync(command.ServiceOrderId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(ServiceOrder), command.ServiceOrderId);

        var now = clock.GetUtcNow();
        var previous = order.ChangeStatus(command.Status, now);
        await serviceOrders.SaveChangesAsync(cancellationToken);

        // Publish after the state is persisted, so subscribers never see uncommitted state.
        await events.PublishStatusChangedAsync(
            new ServiceOrderStatusChanged(order.Id, order.VehicleId, previous, order.Status, now),
            cancellationToken);

        return order;
    }
}
