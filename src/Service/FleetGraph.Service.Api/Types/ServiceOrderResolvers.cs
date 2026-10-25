using FleetGraph.Service.Domain;

namespace FleetGraph.Service.Api.Types;

[ObjectType<ServiceOrderNode>]
public static partial class ServiceOrderResolvers
{
    /// <summary>
    /// Returns only a reference (the key). The gateway fetches the vehicle's fields from the
    /// Vehicles subgraph through its <c>vehicleById</c> lookup.
    /// </summary>
    [BindMember(nameof(ServiceOrderNode.VehicleId))]
    public static VehicleStub GetVehicle([Parent] ServiceOrderNode serviceOrder)
        => new(serviceOrder.VehicleId);

    [BindMember(nameof(ServiceOrderNode.WorkshopId))]
    public static async Task<WorkshopNode?> GetWorkshopAsync(
        [Parent] ServiceOrderNode serviceOrder,
        IWorkshopByIdDataLoader workshopById,
        CancellationToken cancellationToken)
        => await workshopById.LoadAsync(serviceOrder.WorkshopId, cancellationToken);

    [ListSize(AssumedSize = ServiceOrder.MaxLines, RequireOneSlicingArgument = false)]
    public static async Task<ServiceOrderLineNode[]> GetLinesAsync(
        [Parent] ServiceOrderNode serviceOrder,
        ILinesByServiceOrderIdDataLoader linesByServiceOrderId,
        CancellationToken cancellationToken)
        => await linesByServiceOrderId.LoadAsync(serviceOrder.Id, cancellationToken) ?? [];
}

[ObjectType<ServiceOrderLineNode>]
public static partial class ServiceOrderLineResolvers
{
    [BindMember(nameof(ServiceOrderLineNode.PartId))]
    public static PartStub GetPart([Parent] ServiceOrderLineNode line)
        => new(line.PartId);
}
