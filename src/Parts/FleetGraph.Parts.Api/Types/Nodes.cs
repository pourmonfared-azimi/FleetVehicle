using System.Linq.Expressions;
using FleetGraph.Parts.Domain;

namespace FleetGraph.Parts.Api.Types;

[GraphQLName("Part")]
[GraphQLDescription("A spare part in the catalog. Owning subgraph: Parts.")]
public sealed class PartNode
{
    public int Id { get; init; }

    public required string PartNumber { get; init; }

    public required string Name { get; init; }

    public PartCategory Category { get; init; }

    [GraphQLDescription("List price in SEK, excluding VAT.")]
    public decimal UnitPrice { get; init; }
}

[GraphQLName("StockLevel")]
public sealed class StockLevelNode
{
    public int QuantityOnHand { get; init; }

    public int ReorderThreshold { get; init; }

    [GraphQLDescription("True when the quantity on hand is at or below the reorder threshold.")]
    public bool NeedsReorder { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}

public static class PartsMapping
{
    public static readonly Expression<Func<Part, PartNode>> Projection = p => new PartNode
    {
        Id = p.Id,
        PartNumber = p.PartNumber,
        Name = p.Name,
        Category = p.Category,
        UnitPrice = p.UnitPrice
    };

    private static readonly Func<Part, PartNode> s_map = Projection.Compile();

    public static PartNode ToNode(this Part part) => s_map(part);

    public static IQueryable<PartNode> ToNodes(this IQueryable<Part> query) => query.Select(Projection);

    public static StockLevelNode ToNode(this StockLevel stock) => new()
    {
        QuantityOnHand = stock.QuantityOnHand,
        ReorderThreshold = stock.ReorderThreshold,
        NeedsReorder = stock.NeedsReorder,
        UpdatedAt = stock.UpdatedAt
    };
}
