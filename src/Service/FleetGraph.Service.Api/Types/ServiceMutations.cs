using FleetGraph.Service.Application.Commands;
using FleetGraph.Service.Domain;

namespace FleetGraph.Service.Api.Types;

[GraphQLDescription("A part and quantity to reserve for a service order.")]
public sealed record ServiceOrderLineInput(int PartId, int Quantity);

[MutationType]
public static partial class ServiceMutations
{
    [Authorize(Policy = FleetGraphAuth.Policies.CanManageServiceOrders)]
    [Error(typeof(WorkshopNotFoundError))]
    [Error(typeof(ServiceOrderValidationError))]
    [UseMutationConvention(PayloadFieldName = "serviceOrder")]
    public static async Task<ServiceOrderNode> CreateServiceOrderAsync(
        int vehicleId,
        int workshopId,
        string description,
        DateTimeOffset scheduledAt,
        IReadOnlyList<ServiceOrderLineInput> lines,
        CreateServiceOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var order = await handler.HandleAsync(
            new CreateServiceOrderCommand(
                vehicleId,
                workshopId,
                description,
                scheduledAt,
                lines.Select(l => new ServiceOrderLineRequest(l.PartId, l.Quantity)).ToList()),
            cancellationToken);

        return order.ToNode();
    }

    /// <summary>Also publishes <c>serviceOrderStatusChanged</c> to subscribers.</summary>
    [Authorize(Policy = FleetGraphAuth.Policies.CanManageServiceOrders)]
    [Error(typeof(ServiceOrderNotFoundError))]
    [Error(typeof(InvalidServiceOrderStatusTransitionError))]
    [UseMutationConvention(PayloadFieldName = "serviceOrder")]
    public static async Task<ServiceOrderNode> UpdateServiceOrderStatusAsync(
        int serviceOrderId,
        ServiceOrderStatus status,
        UpdateServiceOrderStatusHandler handler,
        CancellationToken cancellationToken)
    {
        var order = await handler.HandleAsync(
            new UpdateServiceOrderStatusCommand(serviceOrderId, status),
            cancellationToken);

        return order.ToNode();
    }
}
