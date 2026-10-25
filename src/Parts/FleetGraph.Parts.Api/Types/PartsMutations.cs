using FleetGraph.Parts.Application.Commands;
using FleetGraph.Parts.Application.Queries;

namespace FleetGraph.Parts.Api.Types;

[MutationType]
public static partial class PartsMutations
{
    /// <summary>Adds (positive) or removes (negative) stock for a part.</summary>
    [Authorize(Policy = FleetGraphAuth.Policies.CanManageStock)]
    [Error(typeof(PartNotFoundError))]
    [Error(typeof(InsufficientStockError))]
    [Error(typeof(StockValidationError))]
    [UseMutationConvention(PayloadFieldName = "part")]
    public static async Task<PartNode> UpdateStockAsync(
        int partId,
        int quantityChange,
        UpdateStockHandler handler,
        PartReadService parts,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UpdateStockCommand(partId, quantityChange), cancellationToken);

        var updated = await parts.GetByIdsAsync([partId], cancellationToken);
        return updated[partId].ToNode();
    }
}
