using FleetGraph.SharedKernel;
using FleetGraph.Vehicles.Application.Abstractions;
using FleetGraph.Vehicles.Domain;

namespace FleetGraph.Vehicles.Application.Commands;

public sealed record RecordMileageCommand(int VehicleId, int MileageKm);

public sealed class RecordMileageHandler(IVehicleRepository vehicles)
{
    public async Task<Vehicle> HandleAsync(
        RecordMileageCommand command,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicles.GetByIdAsync(command.VehicleId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Vehicle), command.VehicleId);

        vehicle.RecordMileage(command.MileageKm);
        await vehicles.SaveChangesAsync(cancellationToken);
        return vehicle;
    }
}
