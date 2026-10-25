using FleetGraph.Parts.Application.Abstractions;
using FleetGraph.Parts.Domain;
using FleetGraph.SharedKernel;

namespace FleetGraph.Parts.Application.Commands;

public sealed record UpdateStockCommand(int PartId, int QuantityChange);

public sealed class UpdateStockHandler(IStockLevelRepository stock, TimeProvider clock)
{
    public async Task<StockLevel> HandleAsync(UpdateStockCommand command, CancellationToken cancellationToken)
    {
        var level = await stock.GetByPartIdAsync(command.PartId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Part), command.PartId);

        level.Adjust(command.QuantityChange, clock.GetUtcNow());
        await stock.SaveChangesAsync(cancellationToken);
        return level;
    }
}
