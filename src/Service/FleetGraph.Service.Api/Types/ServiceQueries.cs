using FleetGraph.Service.Application.Queries;

namespace FleetGraph.Service.Api.Types;

[QueryType]
public static partial class ServiceQueries
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    [GraphQLDescription("Service orders with cursor pagination, filtering and sorting.")]
    public static IQueryable<ServiceOrderNode> GetServiceOrders(ServiceReadService service)
        => service.SearchServiceOrders().ToNodes();

    [Lookup]
    public static async Task<ServiceOrderNode?> GetServiceOrderByIdAsync(
        int id,
        IServiceOrderByIdDataLoader serviceOrderById,
        CancellationToken cancellationToken)
        => await serviceOrderById.LoadAsync(id, cancellationToken);

    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<WorkshopNode> GetWorkshops(ServiceReadService service)
        => service.SearchWorkshops().ToNodes();

    [Lookup]
    public static async Task<WorkshopNode?> GetWorkshopByIdAsync(
        int id,
        IWorkshopByIdDataLoader workshopById,
        CancellationToken cancellationToken)
        => await workshopById.LoadAsync(id, cancellationToken);

    /// <summary>
    /// Internal lookup: hidden from clients, used by the gateway to "enter" this subgraph's
    /// Vehicle type when a query asks for <c>vehicle.serviceOrders</c>.
    /// </summary>
    [Lookup]
    [Internal]
    public static VehicleStub? GetVehicleById(int id) => new(id);

    /// <summary>Internal lookup that declares <c>Part</c> as an entity keyed by <c>id</c>.</summary>
    [Lookup]
    [Internal]
    public static PartStub? GetPartById(int id) => new(id);
}
