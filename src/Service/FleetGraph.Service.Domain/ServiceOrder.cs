using FleetGraph.SharedKernel;

namespace FleetGraph.Service.Domain;

/// <summary>
/// Aggregate root of the Service bounded context. It references a vehicle (Vehicles
/// subgraph) and parts (Parts subgraph) by id only: subgraphs never call each other, the
/// Fusion gateway joins the data.
/// </summary>
public sealed class ServiceOrder : Entity
{
    public const int MaxLines = 20;

    private static readonly IReadOnlyDictionary<ServiceOrderStatus, ServiceOrderStatus[]> s_transitions =
        new Dictionary<ServiceOrderStatus, ServiceOrderStatus[]>
        {
            [ServiceOrderStatus.Scheduled] = [ServiceOrderStatus.InProgress, ServiceOrderStatus.Cancelled],
            [ServiceOrderStatus.InProgress] = [ServiceOrderStatus.AwaitingParts, ServiceOrderStatus.Completed, ServiceOrderStatus.Cancelled],
            [ServiceOrderStatus.AwaitingParts] = [ServiceOrderStatus.InProgress, ServiceOrderStatus.Cancelled],
            [ServiceOrderStatus.Completed] = [],
            [ServiceOrderStatus.Cancelled] = []
        };

    private readonly List<ServiceOrderLine> _lines = [];

    private ServiceOrder()
    {
    }

    public int VehicleId { get; private set; }

    public int WorkshopId { get; private set; }

    public string Description { get; private set; } = default!;

    public ServiceOrderStatus Status { get; private set; }

    public DateTimeOffset ScheduledAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public IReadOnlyList<ServiceOrderLine> Lines => _lines;

    public bool IsOpen => Status is not (ServiceOrderStatus.Completed or ServiceOrderStatus.Cancelled);

    public static ServiceOrder Create(
        int vehicleId,
        int workshopId,
        string description,
        DateTimeOffset scheduledAt,
        IReadOnlyCollection<(int PartId, int Quantity)> lines,
        DateTimeOffset now)
    {
        Guard.Positive(vehicleId, nameof(vehicleId));
        Guard.Positive(workshopId, nameof(workshopId));

        // Store instants in UTC (PostgreSQL timestamptz only accepts offset 0).
        scheduledAt = scheduledAt.ToUniversalTime();
        now = now.ToUniversalTime();

        if (scheduledAt < now.AddMinutes(-5))
        {
            throw new DomainValidationException(
                nameof(scheduledAt),
                "A service order cannot be scheduled in the past.");
        }

        if (lines.Count > MaxLines)
        {
            throw new DomainValidationException(
                nameof(lines),
                $"A service order can have at most {MaxLines} lines.");
        }

        if (lines.GroupBy(l => l.PartId).Any(g => g.Count() > 1))
        {
            throw new DomainValidationException(
                nameof(lines),
                "Each part can only appear once per service order.");
        }

        var order = new ServiceOrder
        {
            VehicleId = vehicleId,
            WorkshopId = workshopId,
            Description = Guard.NotEmpty(description, nameof(description), 500),
            Status = ServiceOrderStatus.Scheduled,
            ScheduledAt = scheduledAt,
            CreatedAt = now
        };

        order._lines.AddRange(lines.Select(l => new ServiceOrderLine(l.PartId, l.Quantity)));
        return order;
    }

    public bool CanTransitionTo(ServiceOrderStatus newStatus)
        => s_transitions[Status].Contains(newStatus);

    /// <returns>The previous status.</returns>
    public ServiceOrderStatus ChangeStatus(ServiceOrderStatus newStatus, DateTimeOffset now)
    {
        if (!CanTransitionTo(newStatus))
        {
            throw new InvalidServiceOrderStatusTransitionException(Status, newStatus);
        }

        var previous = Status;
        Status = newStatus;

        if (newStatus == ServiceOrderStatus.Completed)
        {
            CompletedAt = now.ToUniversalTime();
        }

        return previous;
    }
}
