using System.Text.RegularExpressions;
using FleetGraph.SharedKernel;

namespace FleetGraph.Vehicles.Domain;

/// <summary>
/// Aggregate root of the Vehicles bounded context. The Vehicles subgraph is the owner of the
/// <c>Vehicle</c> entity; other subgraphs only reference it by <see cref="Entity.Id"/>.
/// </summary>
public sealed partial class Vehicle : Entity
{
    public const int MinModelYear = 1980;

    private static readonly IReadOnlyDictionary<VehicleStatus, VehicleStatus[]> s_transitions =
        new Dictionary<VehicleStatus, VehicleStatus[]>
        {
            [VehicleStatus.Active] = [VehicleStatus.InService, VehicleStatus.OutOfService, VehicleStatus.Retired],
            [VehicleStatus.InService] = [VehicleStatus.Active, VehicleStatus.OutOfService],
            [VehicleStatus.OutOfService] = [VehicleStatus.Active, VehicleStatus.InService, VehicleStatus.Retired],
            [VehicleStatus.Retired] = []
        };

    // Required by EF Core.
    private Vehicle()
    {
    }

    public string Vin { get; private set; } = default!;

    public string RegistrationNumber { get; private set; } = default!;

    public string Manufacturer { get; private set; } = default!;

    public string Model { get; private set; } = default!;

    public int ModelYear { get; private set; }

    public int MileageKm { get; private set; }

    public VehicleStatus Status { get; private set; }

    /// <summary>Personal data. Exposed in GraphQL only behind an authorization policy.</summary>
    public string? OwnerEmail { get; private set; }

    public DateTimeOffset RegisteredAt { get; private set; }

    public static Vehicle Register(
        string vin,
        string registrationNumber,
        string manufacturer,
        string model,
        int modelYear,
        int mileageKm,
        string? ownerEmail,
        DateTimeOffset now)
    {
        var normalizedVin = Guard.NotEmpty(vin, nameof(vin), 17).ToUpperInvariant();

        if (!VinPattern().IsMatch(normalizedVin))
        {
            throw new DomainValidationException(
                nameof(vin),
                "VIN must be 17 characters (A-Z, 0-9) and must not contain I, O or Q.");
        }

        if (mileageKm < 0)
        {
            throw new DomainValidationException(nameof(mileageKm), "Mileage must not be negative.");
        }

        if (ownerEmail is not null && !ownerEmail.Contains('@'))
        {
            throw new DomainValidationException(nameof(ownerEmail), "Owner email is not valid.");
        }

        return new Vehicle
        {
            Vin = normalizedVin,
            RegistrationNumber = Guard.NotEmpty(registrationNumber, nameof(registrationNumber), 12)
                .ToUpperInvariant(),
            Manufacturer = Guard.NotEmpty(manufacturer, nameof(manufacturer), 60),
            Model = Guard.NotEmpty(model, nameof(model), 60),
            ModelYear = Guard.InRange(modelYear, MinModelYear, now.Year + 1, nameof(modelYear)),
            MileageKm = mileageKm,
            OwnerEmail = ownerEmail?.Trim(),
            Status = VehicleStatus.Active,
            RegisteredAt = now
        };
    }

    public bool CanTransitionTo(VehicleStatus newStatus)
        => s_transitions[Status].Contains(newStatus);

    /// <summary>Changes the status, enforcing the allowed lifecycle transitions.</summary>
    /// <returns>The previous status.</returns>
    public VehicleStatus ChangeStatus(VehicleStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
        {
            throw new InvalidVehicleStatusTransitionException(Status, newStatus);
        }

        var previous = Status;
        Status = newStatus;
        return previous;
    }

    /// <summary>Odometer readings can only go up.</summary>
    public void RecordMileage(int mileageKm)
    {
        if (mileageKm < MileageKm)
        {
            throw new DomainValidationException(
                nameof(mileageKm),
                $"Mileage cannot decrease (current {MileageKm} km).");
        }

        MileageKm = mileageKm;
    }

    [GeneratedRegex("^[A-HJ-NPR-Z0-9]{17}$")]
    private static partial Regex VinPattern();
}
