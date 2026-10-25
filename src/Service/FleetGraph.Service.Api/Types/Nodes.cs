using System.Linq.Expressions;
using FleetGraph.Service.Domain;

namespace FleetGraph.Service.Api.Types;

[GraphQLName("ServiceOrder")]
[GraphQLDescription("A workshop visit for one vehicle. Owning subgraph: Service.")]
public sealed class ServiceOrderNode
{
    public int Id { get; init; }

    public required string Description { get; init; }

    public ServiceOrderStatus Status { get; init; }

    public DateTimeOffset ScheduledAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    // Replaced by `vehicle` / `workshop` in the schema (see ServiceOrderResolvers).
    public int VehicleId { get; init; }

    public int WorkshopId { get; init; }
}

[GraphQLName("ServiceOrderLine")]
[GraphQLDescription("A part needed for a service order.")]
public sealed class ServiceOrderLineNode
{
    public int Id { get; init; }

    // Replaced by `part` in the schema.
    public int PartId { get; init; }

    public int Quantity { get; init; }
}

[GraphQLName("Workshop")]
[GraphQLDescription("A workshop that performs service orders. Owning subgraph: Service.")]
public sealed class WorkshopNode
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public required string City { get; init; }

    [GraphQLDescription("Number of service bays (parallel capacity).")]
    public int Bays { get; init; }
}

/// <summary>
/// Entity stub: the Service subgraph does not own Vehicle, it only contributes the
/// <c>serviceOrders</c> field to it. Fusion merges this with the Vehicles subgraph's type.
/// </summary>
[GraphQLName("Vehicle")]
public sealed record VehicleStub(int Id)
{
    [ListSize(AssumedSize = 10, RequireOneSlicingArgument = false)]
    [GraphQLDescription("Service orders for this vehicle, newest first. Contributed by the Service subgraph.")]
    public async Task<ServiceOrderNode[]> GetServiceOrdersAsync(
        IServiceOrdersByVehicleIdDataLoader serviceOrdersByVehicleId,
        CancellationToken cancellationToken,
        bool onlyOpen = false)
    {
        var orders = await serviceOrdersByVehicleId.LoadAsync(Id, cancellationToken) ?? [];

        return onlyOpen
            ? orders.Where(o => o.Status is not (ServiceOrderStatus.Completed or ServiceOrderStatus.Cancelled)).ToArray()
            : orders;
    }
}

/// <summary>Entity stub: a reference to a part owned by the Parts subgraph.</summary>
[GraphQLName("Part")]
public sealed record PartStub(int Id);

public static class ServiceMapping
{
    public static readonly Expression<Func<ServiceOrder, ServiceOrderNode>> ServiceOrderProjection =
        o => new ServiceOrderNode
        {
            Id = o.Id,
            Description = o.Description,
            Status = o.Status,
            ScheduledAt = o.ScheduledAt,
            CreatedAt = o.CreatedAt,
            CompletedAt = o.CompletedAt,
            VehicleId = o.VehicleId,
            WorkshopId = o.WorkshopId
        };

    public static readonly Expression<Func<Workshop, WorkshopNode>> WorkshopProjection =
        w => new WorkshopNode
        {
            Id = w.Id,
            Name = w.Name,
            City = w.City,
            Bays = w.Bays
        };

    private static readonly Func<ServiceOrder, ServiceOrderNode> s_serviceOrder = ServiceOrderProjection.Compile();
    private static readonly Func<Workshop, WorkshopNode> s_workshop = WorkshopProjection.Compile();

    public static ServiceOrderNode ToNode(this ServiceOrder order) => s_serviceOrder(order);

    public static WorkshopNode ToNode(this Workshop workshop) => s_workshop(workshop);

    public static ServiceOrderLineNode ToNode(this ServiceOrderLine line)
        => new() { Id = line.Id, PartId = line.PartId, Quantity = line.Quantity };

    public static IQueryable<ServiceOrderNode> ToNodes(this IQueryable<ServiceOrder> query)
        => query.Select(ServiceOrderProjection);

    public static IQueryable<WorkshopNode> ToNodes(this IQueryable<Workshop> query)
        => query.Select(WorkshopProjection);
}
