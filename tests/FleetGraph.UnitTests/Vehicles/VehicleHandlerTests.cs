using FleetGraph.SharedKernel;
using FleetGraph.Vehicles.Application.Abstractions;
using FleetGraph.Vehicles.Application.Commands;
using FleetGraph.Vehicles.Application.Queries;
using FleetGraph.Vehicles.Domain;

namespace FleetGraph.UnitTests.Vehicles;

public class VehicleHandlerTests
{
    private readonly Mock<IVehicleRepository> _repository = new(MockBehavior.Strict);

    private static RegisterVehicleCommand ValidCommand(string vin = "YV1LZ5947W2000099")
        => new(vin, "ZZZ999", "Volvo", "FM", 2023, 10, null);

    [Fact]
    public async Task RegisterVehicle_adds_and_saves_a_new_vehicle()
    {
        _repository.Setup(r => r.VinExistsAsync("YV1LZ5947W2000099", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(r => r.Add(It.IsAny<Vehicle>()));
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RegisterVehicleHandler(_repository.Object, TestClock.Create());

        var vehicle = await handler.HandleAsync(ValidCommand(), CancellationToken.None);

        Assert.Equal("YV1LZ5947W2000099", vehicle.Vin);
        Assert.Equal(TestClock.Now, vehicle.RegisteredAt);
        _repository.Verify(r => r.Add(It.Is<Vehicle>(v => v.Vin == "YV1LZ5947W2000099")), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterVehicle_rejects_duplicate_vin_without_saving()
    {
        _repository.Setup(r => r.VinExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RegisterVehicleHandler(_repository.Object, TestClock.Create());

        var ex = await Assert.ThrowsAsync<DuplicateVinException>(
            () => handler.HandleAsync(ValidCommand(), CancellationToken.None));

        Assert.Equal("YV1LZ5947W2000099", ex.Vin);
        _repository.Verify(r => r.Add(It.IsAny<Vehicle>()), Times.Never);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterVehicle_validates_before_touching_the_database()
    {
        // Strict mock: any repository call would throw a MockException.
        var handler = new RegisterVehicleHandler(_repository.Object, TestClock.Create());

        await Assert.ThrowsAsync<DomainValidationException>(
            () => handler.HandleAsync(ValidCommand(vin: "INVALID"), CancellationToken.None));
    }

    [Fact]
    public async Task ChangeVehicleStatus_throws_not_found_for_unknown_vehicle()
    {
        _repository.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var handler = new ChangeVehicleStatusHandler(_repository.Object);

        var ex = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.HandleAsync(new ChangeVehicleStatusCommand(42, VehicleStatus.InService), CancellationToken.None));

        Assert.Equal("VEHICLE_NOT_FOUND", ex.Code);
        Assert.Equal(42, ex.EntityId);
    }

    [Fact]
    public async Task ChangeVehicleStatus_changes_and_saves()
    {
        var vehicle = Vehicle.Register("YV1LZ5947W2000001", "ABC123", "Volvo", "FH", 2022, 0, null, TestClock.Now);
        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(vehicle);
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new ChangeVehicleStatusHandler(_repository.Object);

        var result = await handler.HandleAsync(
            new ChangeVehicleStatusCommand(1, VehicleStatus.InService),
            CancellationToken.None);

        Assert.Equal(VehicleStatus.InService, result.Status);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReadService_skips_the_database_for_an_empty_batch()
    {
        var service = new VehicleReadService(_repository.Object);

        var result = await service.GetByIdsAsync([], CancellationToken.None);

        Assert.Empty(result);
        _repository.VerifyNoOtherCalls();
    }
}
