namespace FleetGraph.Service.Domain;

public enum ServiceOrderStatus
{
    Scheduled,
    InProgress,
    AwaitingParts,
    Completed,
    Cancelled
}
