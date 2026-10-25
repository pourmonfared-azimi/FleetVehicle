namespace FleetGraph.Parts.Api.Types;

[ObjectType<PartNode>]
public static partial class PartResolvers
{
    /// <summary>
    /// Batched with a DataLoader: a service order with 10 lines -> 10 parts -> one stock query.
    /// </summary>
    public static async Task<StockLevelNode?> GetStockLevelAsync(
        [Parent] PartNode part,
        IStockLevelByPartIdDataLoader stockLevelByPartId,
        CancellationToken cancellationToken)
        => await stockLevelByPartId.LoadAsync(part.Id, cancellationToken);
}
