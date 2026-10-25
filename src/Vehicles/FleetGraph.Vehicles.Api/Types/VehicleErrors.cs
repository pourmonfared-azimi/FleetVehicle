using FleetGraph.SharedKernel;
using FleetGraph.Vehicles.Domain;

namespace FleetGraph.Vehicles.Api.Types;

// Typed mutation errors. Each class maps exactly one domain exception through its
// CreateErrorFrom factory, so internal exception details never reach the client.

public sealed class VehicleNotFoundError : IUserError
{
    private VehicleNotFoundError(int vehicleId, string code, string message)
        => (VehicleId, Code, Message) = (vehicleId, code, message);

    public int VehicleId { get; }

    public string Code { get; }

    public string Message { get; }

    public static VehicleNotFoundError CreateErrorFrom(EntityNotFoundException ex)
        => new(ex.EntityId, ex.Code, ex.Message);
}

public sealed class VehicleValidationError : IUserError
{
    private VehicleValidationError(string field, string code, string message)
        => (Field, Code, Message) = (field, code, message);

    [GraphQLDescription("The input field that failed validation.")]
    public string Field { get; }

    public string Code { get; }

    public string Message { get; }

    public static VehicleValidationError CreateErrorFrom(DomainValidationException ex)
        => new(ex.Field, ex.Code, ex.Message);
}

public sealed class DuplicateVinError : IUserError
{
    private DuplicateVinError(string vin, string code, string message)
        => (Vin, Code, Message) = (vin, code, message);

    public string Vin { get; }

    public string Code { get; }

    public string Message { get; }

    public static DuplicateVinError CreateErrorFrom(DuplicateVinException ex)
        => new(ex.Vin, ex.Code, ex.Message);
}

public sealed class InvalidVehicleStatusTransitionError : IUserError
{
    private InvalidVehicleStatusTransitionError(
        VehicleStatus from,
        VehicleStatus to,
        string code,
        string message)
        => (From, To, Code, Message) = (from, to, code, message);

    public VehicleStatus From { get; }

    public VehicleStatus To { get; }

    public string Code { get; }

    public string Message { get; }

    public static InvalidVehicleStatusTransitionError CreateErrorFrom(
        InvalidVehicleStatusTransitionException ex)
        => new(ex.From, ex.To, ex.Code, ex.Message);
}
