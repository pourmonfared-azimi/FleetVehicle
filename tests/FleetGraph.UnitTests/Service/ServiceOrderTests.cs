using FleetGraph.Service.Domain;
using FleetGraph.SharedKernel;

namespace FleetGraph.UnitTests.Service;

public class ServiceOrderTests
{
    private static ServiceOrder Create(params (int PartId, int Quantity)[] lines)
        => ServiceOrder.Create(1, 1, "Brake inspection", TestClock.Now.AddDays(1), lines, TestClock.Now);

    [Fact]
    public void Create_starts_scheduled_with_lines()
    {
        var order = Create((3, 2), (1, 1));

        Assert.Equal(ServiceOrderStatus.Scheduled, order.Status);
        Assert.Equal(2, order.Lines.Count);
        Assert.True(order.IsOpen);
        Assert.Null(order.CompletedAt);
    }

    [Fact]
    public void Create_rejects_a_schedule_in_the_past()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            ServiceOrder.Create(1, 1, "x", TestClock.Now.AddDays(-1), [], TestClock.Now));

        Assert.Equal("scheduledAt", ex.Field);
    }

    [Fact]
    public void Create_rejects_the_same_part_twice()
    {
        var ex = Assert.Throws<DomainValidationException>(() => Create((3, 1), (3, 2)));
        Assert.Equal("lines", ex.Field);
    }

    [Fact]
    public void Create_rejects_too_many_lines()
    {
        var lines = Enumerable.Range(1, ServiceOrder.MaxLines + 1).Select(i => (i, 1)).ToArray();

        var ex = Assert.Throws<DomainValidationException>(() => Create(lines));
        Assert.Equal("lines", ex.Field);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Create_rejects_invalid_quantity(int quantity)
    {
        var ex = Assert.Throws<DomainValidationException>(() => Create((1, quantity)));
        Assert.Equal("quantity", ex.Field);
    }

    [Fact]
    public void Completing_sets_completed_at_and_closes_the_order()
    {
        var order = Create();
        var completedAt = TestClock.Now.AddHours(5);

        order.ChangeStatus(ServiceOrderStatus.InProgress, TestClock.Now);
        var previous = order.ChangeStatus(ServiceOrderStatus.Completed, completedAt);

        Assert.Equal(ServiceOrderStatus.InProgress, previous);
        Assert.Equal(completedAt, order.CompletedAt);
        Assert.False(order.IsOpen);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.Completed)]      // must be in progress first
    [InlineData(ServiceOrderStatus.AwaitingParts)]  // must be in progress first
    [InlineData(ServiceOrderStatus.Scheduled)]      // no self transition
    public void Scheduled_order_rejects_invalid_transitions(ServiceOrderStatus target)
    {
        var order = Create();

        var ex = Assert.Throws<InvalidServiceOrderStatusTransitionException>(
            () => order.ChangeStatus(target, TestClock.Now));

        Assert.Equal(ServiceOrderStatus.Scheduled, ex.From);
        Assert.Equal(target, ex.To);
    }

    [Fact]
    public void Cancelled_is_terminal()
    {
        var order = Create();
        order.ChangeStatus(ServiceOrderStatus.Cancelled, TestClock.Now);

        Assert.False(order.CanTransitionTo(ServiceOrderStatus.InProgress));
        Assert.Throws<InvalidServiceOrderStatusTransitionException>(
            () => order.ChangeStatus(ServiceOrderStatus.InProgress, TestClock.Now));
    }
}
