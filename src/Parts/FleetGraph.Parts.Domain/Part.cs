using FleetGraph.SharedKernel;

namespace FleetGraph.Parts.Domain;

/// <summary>Catalog entry. Owning subgraph: Parts.</summary>
public sealed class Part : Entity
{
    private Part()
    {
    }

    public string PartNumber { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public PartCategory Category { get; private set; }

    public decimal UnitPrice { get; private set; }

    public static Part Create(string partNumber, string name, PartCategory category, decimal unitPrice)
        => new()
        {
            PartNumber = Guard.NotEmpty(partNumber, nameof(partNumber), 30).ToUpperInvariant(),
            Name = Guard.NotEmpty(name, nameof(name), 120),
            Category = category,
            UnitPrice = Guard.NotNegative(unitPrice, nameof(unitPrice))
        };
}
