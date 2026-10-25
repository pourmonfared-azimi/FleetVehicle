using FleetGraph.Parts.Domain;
using FleetGraph.SharedKernel;

namespace FleetGraph.Parts.Api.Types;

public sealed class PartNotFoundError : IUserError
{
    private PartNotFoundError(int partId, string code, string message)
        => (PartId, Code, Message) = (partId, code, message);

    public int PartId { get; }

    public string Code { get; }

    public string Message { get; }

    public static PartNotFoundError CreateErrorFrom(EntityNotFoundException ex)
        => new(ex.EntityId, ex.Code, ex.Message);
}

public sealed class InsufficientStockError : IUserError
{
    private InsufficientStockError(int partId, int quantityOnHand, int requested, string code, string message)
        => (PartId, QuantityOnHand, Requested, Code, Message) = (partId, quantityOnHand, requested, code, message);

    public int PartId { get; }

    public int QuantityOnHand { get; }

    public int Requested { get; }

    public string Code { get; }

    public string Message { get; }

    public static InsufficientStockError CreateErrorFrom(InsufficientStockException ex)
        => new(ex.PartId, ex.QuantityOnHand, ex.Requested, ex.Code, ex.Message);
}

public sealed class StockValidationError : IUserError
{
    private StockValidationError(string field, string code, string message)
        => (Field, Code, Message) = (field, code, message);

    public string Field { get; }

    public string Code { get; }

    public string Message { get; }

    public static StockValidationError CreateErrorFrom(DomainValidationException ex)
        => new(ex.Field, ex.Code, ex.Message);
}
