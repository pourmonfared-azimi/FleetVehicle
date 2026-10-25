using FleetGraph.SharedKernel;
using FleetGraph.Vehicles.Application.Abstractions;
using FleetGraph.Vehicles.Domain;

namespace FleetGraph.Vehicles.Application.Commands;

public sealed record ChangeVehicleStatusCommand(int VehicleId, VehicleStatus Status);

public sealed class ChangeVehicleStatusHandler(IVehicleRepository vehicles)
{
    public async Task<Vehicle> HandleAsync(
        ChangeVehicleStatusCommand command,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicles.GetByIdAsync(command.VehicleId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Vehicle), command.VehicleId);

        vehicle.ChangeStatus(command.Status);
        await vehicles.SaveChangesAsync(cancellationToken);
        return vehicle;
    }
}
