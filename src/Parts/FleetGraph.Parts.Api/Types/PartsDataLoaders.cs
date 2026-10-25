using FleetGraph.Parts.Application.Queries;

namespace FleetGraph.Parts.Api.Types;

public static class PartsDataLoaders
{
    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, PartNode>> GetPartByIdAsync(
        IReadOnlyList<int> ids,
        PartReadService parts,
        CancellationToken cancellationToken)
        => (await parts.GetByIdsAsync(ids, cancellationToken))
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToNode());

    [DataLoader]
    public static async Task<IReadOnlyDictionary<int, StockLevelNode>> GetStockLevelByPartIdAsync(
        IReadOnlyList<int> partIds,
        PartReadService parts,
        CancellationToken cancellationToken)
        => (await parts.GetStockByPartIdsAsync(partIds, cancellationToken))
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToNode());
}
