using FleetGraph.SharedKernel;

namespace FleetGraph.Parts.Domain;

public sealed class InsufficientStockException(int partId, int quantityOnHand, int requested)
    : DomainException(
        "INSUFFICIENT_STOCK",
        $"Cannot remove {requested} unit(s) of part {partId}: only {quantityOnHand} on hand.")
{
    public int PartId { get; } = partId;

    public int QuantityOnHand { get; } = quantityOnHand;

    public int Requested { get; } = requested;
}

/// <summary>
/// Stock for one part. Modelled as its own entity (and table) so that reading the catalog
/// does not lock stock rows, and to show a DataLoader for <c>Part.stockLevel</c>.
/// </summary>
public sealed class StockLevel : Entity
{
    public const int MaxAdjustment = 10_000;

    private StockLevel()
    {
    }

    public int PartId { get; private set; }

    public int QuantityOnHand { get; private set; }

    public int ReorderThreshold { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool NeedsReorder => QuantityOnHand <= ReorderThreshold;

    public static StockLevel Create(int partId, int quantityOnHand, int reorderThreshold, DateTimeOffset now)
        => new()
        {
            PartId = Guard.Positive(partId, nameof(partId)),
            QuantityOnHand = Guard.InRange(quantityOnHand, 0, int.MaxValue, nameof(quantityOnHand)),
            ReorderThreshold = Guard.InRange(reorderThreshold, 0, int.MaxValue, nameof(reorderThreshold)),
            UpdatedAt = now
        };

    /// <summary>Adds (positive) or removes (negative) stock.</summary>
    public void Adjust(int quantityChange, DateTimeOffset now)
    {
        if (quantityChange == 0)
        {
            throw new DomainValidationException(nameof(quantityChange), "Quantity change must not be zero.");
        }

        if (Math.Abs(quantityChange) > MaxAdjustment)
        {
            throw new DomainValidationException(
                nameof(quantityChange),
                $"A single adjustment must be at most {MaxAdjustment} units.");
        }

        if (QuantityOnHand + quantityChange < 0)
        {
            throw new InsufficientStockException(PartId, QuantityOnHand, -quantityChange);
        }

        QuantityOnHand += quantityChange;
        UpdatedAt = now;
    }
}
