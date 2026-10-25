using FleetGraph.Vehicles.Application.Abstractions;
using FleetGraph.Vehicles.Domain;

namespace FleetGraph.Vehicles.Application.Commands;

public sealed record RegisterVehicleCommand(
    string Vin,
    string RegistrationNumber,
    string Manufacturer,
    string Model,
    int ModelYear,
    int MileageKm,
    string? OwnerEmail);

public sealed class RegisterVehicleHandler(IVehicleRepository vehicles, TimeProvider clock)
{
    public async Task<Vehicle> HandleAsync(
        RegisterVehicleCommand command,
        CancellationToken cancellationToken)
    {
        // Domain validation first (cheap, no I/O) ...
        var vehicle = Vehicle.Register(
            command.Vin,
            command.RegistrationNumber,
            command.Manufacturer,
            command.Model,
            command.ModelYear,
            command.MileageKm,
            command.OwnerEmail,
            clock.GetUtcNow());

        // ... then the rule that needs the database.
        if (await vehicles.VinExistsAsync(vehicle.Vin, cancellationToken))
        {
            throw new DuplicateVinException(vehicle.Vin);
        }

        vehicles.Add(vehicle);
        await vehicles.SaveChangesAsync(cancellationToken);
        return vehicle;
    }
}
