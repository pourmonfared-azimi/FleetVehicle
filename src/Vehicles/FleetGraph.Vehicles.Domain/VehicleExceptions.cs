using FleetGraph.SharedKernel;

namespace FleetGraph.Vehicles.Domain;

public sealed class DuplicateVinException(string vin)
    : DomainException("DUPLICATE_VIN", $"A vehicle with VIN '{vin}' is already registered.")
{
    public string Vin { get; } = vin;
}

public sealed class InvalidVehicleStatusTransitionException(VehicleStatus from, VehicleStatus to)
    : DomainException(
        "INVALID_STATUS_TRANSITION",
        $"A vehicle cannot change status from {from} to {to}.")
{
    public VehicleStatus From { get; } = from;

    public VehicleStatus To { get; } = to;
}
