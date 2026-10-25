using FleetGraph.Service.Domain;
using StatusChangedMessage = FleetGraph.Service.Application.Contracts.ServiceOrderStatusChanged;

namespace FleetGraph.Service.Api.Types;

[GraphQLName("ServiceOrderStatusChangedEvent")]
public sealed class ServiceOrderStatusChangedEvent(StatusChangedMessage message)
{
    // Replaced by `serviceOrder` / `vehicle` in the schema.
    public int ServiceOrderId { get; } = message.ServiceOrderId;

    public int VehicleId { get; } = message.VehicleId;

    public ServiceOrderStatus PreviousStatus { get; } = message.PreviousStatus;

    public ServiceOrderStatus NewStatus { get; } = message.NewStatus;

    public DateTimeOffset ChangedAt { get; } = message.ChangedAt;
}

[ObjectType<ServiceOrderStatusChangedEvent>]
public static partial class ServiceOrderStatusChangedEventResolvers
{
    [BindMember(nameof(ServiceOrderStatusChangedEvent.ServiceOrderId))]
    public static async Task<ServiceOrderNode?> GetServiceOrderAsync(
        [Parent] ServiceOrderStatusChangedEvent @event,
        IServiceOrderByIdDataLoader serviceOrderById,
        CancellationToken cancellationToken)
        => await serviceOrderById.LoadAsync(@event.ServiceOrderId, cancellationToken);

    [BindMember(nameof(ServiceOrderStatusChangedEvent.VehicleId))]
    public static VehicleStub GetVehicle([Parent] ServiceOrderStatusChangedEvent @event)
        => new(@event.VehicleId);
}

[SubscriptionType]
public static partial class ServiceSubscriptions
{
    public const string StatusChangedTopic = "ServiceOrderStatusChanged";

    /// <summary>
    /// Pushed every time a service order changes status. The gateway consumes this stream
    /// from the subgraph over SSE and resolves cross-subgraph fields (e.g. the vehicle) per event.
    /// </summary>
    [Subscribe]
    [Topic(StatusChangedTopic)]
    public static ServiceOrderStatusChangedEvent ServiceOrderStatusChanged(
        [EventMessage] StatusChangedMessage message)
        => new(message);
}
