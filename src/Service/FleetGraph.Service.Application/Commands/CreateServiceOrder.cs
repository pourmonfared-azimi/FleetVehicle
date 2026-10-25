using FleetGraph.Service.Application.Abstractions;
using FleetGraph.Service.Domain;
using FleetGraph.SharedKernel;

namespace FleetGraph.Service.Application.Commands;

public sealed record ServiceOrderLineRequest(int PartId, int Quantity);

public sealed record CreateServiceOrderCommand(
    int VehicleId,
    int WorkshopId,
    string Description,
    DateTimeOffset ScheduledAt,
    IReadOnlyList<ServiceOrderLineRequest> Lines);

public sealed class CreateServiceOrderHandler(
    IServiceOrderRepository serviceOrders,
    IWorkshopRepository workshops,
    TimeProvider clock)
{
    public async Task<ServiceOrder> HandleAsync(
        CreateServiceOrderCommand command,
        CancellationToken cancellationToken)
    {
        var order = ServiceOrder.Create(
            command.VehicleId,
            command.WorkshopId,
            command.Description,
            command.ScheduledAt,
            command.Lines.Select(l => (l.PartId, l.Quantity)).ToList(),
            clock.GetUtcNow());

        // The workshop is owned by this subgraph, so we can check it. The vehicle and the
        // parts are owned by other subgraphs: by design we do not call them synchronously
        // (see docs/adr/0001). The gateway returns null for unknown references.
        if (!await workshops.ExistsAsync(command.WorkshopId, cancellationToken))
        {
            throw new EntityNotFoundException(nameof(Workshop), command.WorkshopId);
        }

        serviceOrders.Add(order);
        await serviceOrders.SaveChangesAsync(cancellationToken);
        return order;
    }
}
