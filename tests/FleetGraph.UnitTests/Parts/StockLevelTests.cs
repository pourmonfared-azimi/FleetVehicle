using FleetGraph.Parts.Application.Abstractions;
using FleetGraph.Parts.Application.Commands;
using FleetGraph.Parts.Domain;
using FleetGraph.SharedKernel;

namespace FleetGraph.UnitTests.Parts;

public class StockLevelTests
{
    private static StockLevel Stock(int onHand = 10, int reorderAt = 3)
        => StockLevel.Create(partId: 1, onHand, reorderAt, TestClock.Now);

    [Fact]
    public void Adjust_adds_and_removes_stock()
    {
        var stock = Stock();
        var later = TestClock.Now.AddMinutes(1);

        stock.Adjust(5, later);
        stock.Adjust(-12, later);

        Assert.Equal(3, stock.QuantityOnHand);
        Assert.Equal(later, stock.UpdatedAt);
    }

    [Fact]
    public void Adjust_rejects_removing_more_than_on_hand()
    {
        var stock = Stock(onHand: 2);

        var ex = Assert.Throws<InsufficientStockException>(() => stock.Adjust(-5, TestClock.Now));

        Assert.Equal(2, ex.QuantityOnHand);
        Assert.Equal(5, ex.Requested);
        Assert.Equal("INSUFFICIENT_STOCK", ex.Code);
        Assert.Equal(2, stock.QuantityOnHand); // unchanged
    }

    [Theory]
    [InlineData(0)]
    [InlineData(StockLevel.MaxAdjustment + 1)]
    [InlineData(-(StockLevel.MaxAdjustment + 1))]
    public void Adjust_rejects_invalid_changes(int change)
    {
        var ex = Assert.Throws<DomainValidationException>(() => Stock(onHand: 50_000).Adjust(change, TestClock.Now));
        Assert.Equal("quantityChange", ex.Field);
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(0, true)]
    public void NeedsReorder_when_at_or_below_threshold(int onHand, bool expected)
        => Assert.Equal(expected, Stock(onHand, reorderAt: 3).NeedsReorder);
}

public class UpdateStockHandlerTests
{
    private readonly Mock<IStockLevelRepository> _stock = new();

    [Fact]
    public async Task Adjusts_and_saves()
    {
        var level = StockLevel.Create(4, 1, 1, TestClock.Now);
        _stock.Setup(s => s.GetByPartIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(level);

        var handler = new UpdateStockHandler(_stock.Object, TestClock.Create());

        var result = await handler.HandleAsync(new UpdateStockCommand(4, 9), CancellationToken.None);

        Assert.Equal(10, result.QuantityOnHand);
        _stock.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Unknown_part_throws_not_found()
    {
        _stock.Setup(s => s.GetByPartIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockLevel?)null);

        var handler = new UpdateStockHandler(_stock.Object, TestClock.Create());

        var ex = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.HandleAsync(new UpdateStockCommand(999, 1), CancellationToken.None));

        Assert.Equal("PART_NOT_FOUND", ex.Code);
        _stock.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Insufficient_stock_is_not_saved()
    {
        _stock.Setup(s => s.GetByPartIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StockLevel.Create(4, 1, 1, TestClock.Now));

        var handler = new UpdateStockHandler(_stock.Object, TestClock.Create());

        await Assert.ThrowsAsync<InsufficientStockException>(
            () => handler.HandleAsync(new UpdateStockCommand(4, -5), CancellationToken.None));

        _stock.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
