using FleetGraph.Service.Application.Abstractions;
using FleetGraph.Service.Application.Commands;
using FleetGraph.Service.Application.Contracts;
using FleetGraph.Service.Application.Queries;
using FleetGraph.Service.Domain;
using FleetGraph.SharedKernel;

namespace FleetGraph.UnitTests.Service;

public class ServiceOrderHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _orders = new();
    private readonly Mock<IWorkshopRepository> _workshops = new();
    private readonly Mock<IServiceOrderEventPublisher> _events = new();

    private static CreateServiceOrderCommand Command(int workshopId = 1)
        => new(
            VehicleId: 7,
            WorkshopId: workshopId,
            Description: "Replace air dryer",
            ScheduledAt: TestClock.Now.AddDays(2),
            Lines: [new ServiceOrderLineRequest(8, 1), new ServiceOrderLineRequest(2, 4)]);

    [Fact]
    public async Task CreateServiceOrder_persists_the_order()
    {
        _workshops.Setup(w => w.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new CreateServiceOrderHandler(_orders.Object, _workshops.Object, TestClock.Create());

        var order = await handler.HandleAsync(Command(), CancellationToken.None);

        Assert.Equal(7, order.VehicleId);
        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(TestClock.Now, order.CreatedAt);
        _orders.Verify(o => o.Add(order), Times.Once);
        _orders.Verify(o => o.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateServiceOrder_fails_for_unknown_workshop()
    {
        _workshops.Setup(w => w.ExistsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new CreateServiceOrderHandler(_orders.Object, _workshops.Object, TestClock.Create());

        var ex = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.HandleAsync(Command(workshopId: 99), CancellationToken.None));

        Assert.Equal("WORKSHOP_NOT_FOUND", ex.Code);
        _orders.Verify(o => o.Add(It.IsAny<ServiceOrder>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_saves_then_publishes_the_change()
    {
        var order = ServiceOrder.Create(3, 1, "Calibration", TestClock.Now.AddDays(1), [], TestClock.Now);
        var calls = new List<string>();

        _orders.Setup(o => o.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _orders.Setup(o => o.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("save"))
            .Returns(Task.CompletedTask);
        _events.Setup(e => e.PublishStatusChangedAsync(It.IsAny<ServiceOrderStatusChanged>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("publish"))
            .Returns(ValueTask.CompletedTask);

        var handler = new UpdateServiceOrderStatusHandler(_orders.Object, _events.Object, TestClock.Create());

        await handler.HandleAsync(
            new UpdateServiceOrderStatusCommand(5, ServiceOrderStatus.InProgress),
            CancellationToken.None);

        Assert.Equal(new[] { "save", "publish" }, calls);
        _events.Verify(e => e.PublishStatusChangedAsync(
            It.Is<ServiceOrderStatusChanged>(m =>
                m.VehicleId == 3
                && m.PreviousStatus == ServiceOrderStatus.Scheduled
                && m.NewStatus == ServiceOrderStatus.InProgress
                && m.ChangedAt == TestClock.Now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_does_not_publish_when_the_transition_is_invalid()
    {
        var order = ServiceOrder.Create(3, 1, "Calibration", TestClock.Now.AddDays(1), [], TestClock.Now);
        _orders.Setup(o => o.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var handler = new UpdateServiceOrderStatusHandler(_orders.Object, _events.Object, TestClock.Create());

        await Assert.ThrowsAsync<InvalidServiceOrderStatusTransitionException>(() => handler.HandleAsync(
            new UpdateServiceOrderStatusCommand(5, ServiceOrderStatus.Completed),
            CancellationToken.None));

        _orders.Verify(o => o.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _events.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateStatus_throws_not_found_for_unknown_order()
    {
        _orders.Setup(o => o.GetByIdAsync(404, It.IsAny<CancellationToken>())).ReturnsAsync((ServiceOrder?)null);

        var handler = new UpdateServiceOrderStatusHandler(_orders.Object, _events.Object, TestClock.Create());

        var ex = await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(
            new UpdateServiceOrderStatusCommand(404, ServiceOrderStatus.InProgress),
            CancellationToken.None));

        Assert.Equal("SERVICE_ORDER_NOT_FOUND", ex.Code);
    }

    [Fact]
    public async Task ReadService_groups_service_orders_by_vehicle_newest_first()
    {
        var older = ServiceOrder.Create(1, 1, "older", TestClock.Now.AddDays(1), [], TestClock.Now);
        var newer = ServiceOrder.Create(1, 1, "newer", TestClock.Now.AddDays(5), [], TestClock.Now);
        var other = ServiceOrder.Create(2, 1, "other", TestClock.Now.AddDays(2), [], TestClock.Now);

        _orders.Setup(o => o.GetByVehicleIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceOrder> { older, other, newer });

        var service = new ServiceReadService(_orders.Object, _workshops.Object);

        var result = await service.GetServiceOrdersByVehicleIdsAsync([1, 2], CancellationToken.None);

        Assert.Equal(new[] { "newer", "older" }, result[1].Select(o => o.Description));
        Assert.Single(result[2]);
    }
}
