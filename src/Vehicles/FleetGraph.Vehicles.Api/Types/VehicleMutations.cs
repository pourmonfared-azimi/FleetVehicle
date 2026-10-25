using FleetGraph.Vehicles.Application.Commands;
using FleetGraph.Vehicles.Domain;

namespace FleetGraph.Vehicles.Api.Types;

[MutationType]
public static partial class VehicleMutations
{
    [Authorize(Roles = [FleetGraphAuth.Roles.FleetManager])]
    [Error(typeof(VehicleValidationError))]
    [Error(typeof(DuplicateVinError))]
    [UseMutationConvention(PayloadFieldName = "vehicle")]
    public static async Task<VehicleNode> RegisterVehicleAsync(
        string vin,
        string registrationNumber,
        string manufacturer,
        string model,
        int modelYear,
        int mileageKm,
        string? ownerEmail,
        RegisterVehicleHandler handler,
        CancellationToken cancellationToken)
    {
        var vehicle = await handler.HandleAsync(
            new RegisterVehicleCommand(
                vin,
                registrationNumber,
                manufacturer,
                model,
                modelYear,
                mileageKm,
                ownerEmail),
            cancellationToken);

        return vehicle.ToNode();
    }

    [Authorize(Roles = [FleetGraphAuth.Roles.FleetManager, FleetGraphAuth.Roles.Technician])]
    [Error(typeof(VehicleNotFoundError))]
    [Error(typeof(InvalidVehicleStatusTransitionError))]
    [UseMutationConvention(PayloadFieldName = "vehicle")]
    public static async Task<VehicleNode> ChangeVehicleStatusAsync(
        int vehicleId,
        VehicleStatus status,
        ChangeVehicleStatusHandler handler,
        CancellationToken cancellationToken)
    {
        var vehicle = await handler.HandleAsync(
            new ChangeVehicleStatusCommand(vehicleId, status),
            cancellationToken);

        return vehicle.ToNode();
    }

    [Authorize(Roles = [FleetGraphAuth.Roles.FleetManager, FleetGraphAuth.Roles.Technician])]
    [Error(typeof(VehicleNotFoundError))]
    [Error(typeof(VehicleValidationError))]
    [UseMutationConvention(PayloadFieldName = "vehicle")]
    public static async Task<VehicleNode> RecordMileageAsync(
        int vehicleId,
        int mileageKm,
        RecordMileageHandler handler,
        CancellationToken cancellationToken)
    {
        var vehicle = await handler.HandleAsync(
            new RecordMileageCommand(vehicleId, mileageKm),
            cancellationToken);

        return vehicle.ToNode();
    }
}
