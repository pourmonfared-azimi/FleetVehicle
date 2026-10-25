using FleetGraph.SharedKernel;

namespace FleetGraph.Service.Domain;

/// <summary>A part (owned by the Parts subgraph, referenced by id) needed for a service order.</summary>
public sealed class ServiceOrderLine : Entity
{
    private ServiceOrderLine()
    {
    }

    internal ServiceOrderLine(int partId, int quantity)
    {
        PartId = Guard.Positive(partId, nameof(partId));
        Quantity = Guard.InRange(quantity, 1, 1000, nameof(quantity));
    }

    public int ServiceOrderId { get; private set; }

    public int PartId { get; private set; }

    public int Quantity { get; private set; }
}
