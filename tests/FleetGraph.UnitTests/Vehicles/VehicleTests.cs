using FleetGraph.SharedKernel;
using FleetGraph.Vehicles.Domain;

namespace FleetGraph.UnitTests.Vehicles;

public class VehicleTests
{
    private static Vehicle Create(string vin = "yv1lz5947w2000001", int modelYear = 2022)
        => Vehicle.Register(vin, " abc123 ", "Volvo", "FH16", modelYear, 1000, "owner@example.com", TestClock.Now);

    [Fact]
    public void Register_normalizes_input_and_starts_active()
    {
        var vehicle = Create();

        Assert.Equal("YV1LZ5947W2000001", vehicle.Vin);
        Assert.Equal("ABC123", vehicle.RegistrationNumber);
        Assert.Equal(VehicleStatus.Active, vehicle.Status);
        Assert.Equal(TestClock.Now, vehicle.RegisteredAt);
    }

    [Theory]
    [InlineData("SHORT")]
    [InlineData("YV1LZ5947W200000I")] // I is not allowed
    [InlineData("YV1LZ5947W200000O")] // O is not allowed
    [InlineData("YV1LZ5947W200000Q")] // Q is not allowed
    [InlineData("")]
    public void Register_rejects_invalid_vin(string vin)
    {
        var ex = Assert.Throws<DomainValidationException>(() => Create(vin));
        Assert.Equal("vin", ex.Field);
        Assert.Equal("VALIDATION_FAILED", ex.Code);
    }

    [Theory]
    [InlineData(1979)]
    [InlineData(2028)]
    public void Register_rejects_model_year_out_of_range(int modelYear)
    {
        var ex = Assert.Throws<DomainValidationException>(() => Create(modelYear: modelYear));
        Assert.Equal("modelYear", ex.Field);
    }

    [Fact]
    public void Register_rejects_invalid_owner_email()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            Vehicle.Register("YV1LZ5947W2000001", "ABC123", "Volvo", "FH", 2022, 0, "not-an-email", TestClock.Now));

        Assert.Equal("ownerEmail", ex.Field);
    }

    [Theory]
    [InlineData(VehicleStatus.InService)]
    [InlineData(VehicleStatus.OutOfService)]
    [InlineData(VehicleStatus.Retired)]
    public void ChangeStatus_allows_valid_transitions_and_returns_previous(VehicleStatus target)
    {
        var vehicle = Create();

        var previous = vehicle.ChangeStatus(target);

        Assert.Equal(VehicleStatus.Active, previous);
        Assert.Equal(target, vehicle.Status);
    }

    [Fact]
    public void Retired_is_a_terminal_status()
    {
        var vehicle = Create();
        vehicle.ChangeStatus(VehicleStatus.Retired);

        var ex = Assert.Throws<InvalidVehicleStatusTransitionException>(
            () => vehicle.ChangeStatus(VehicleStatus.Active));

        Assert.Equal(VehicleStatus.Retired, ex.From);
        Assert.Equal(VehicleStatus.Active, ex.To);
        Assert.Equal("INVALID_STATUS_TRANSITION", ex.Code);
    }

    [Fact]
    public void RecordMileage_cannot_decrease()
    {
        var vehicle = Create();
        vehicle.RecordMileage(5000);

        Assert.Throws<DomainValidationException>(() => vehicle.RecordMileage(4999));
        Assert.Equal(5000, vehicle.MileageKm);
    }
}
