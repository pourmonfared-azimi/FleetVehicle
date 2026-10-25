namespace FleetGraph.UnitTests;

internal static class TestClock
{
    public static readonly DateTimeOffset Now = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    /// <summary>A mocked <see cref="TimeProvider"/> that always returns <see cref="Now"/>.</summary>
    public static TimeProvider Create(DateTimeOffset? now = null)
    {
        var clock = new Mock<TimeProvider>();
        clock.Setup(c => c.GetUtcNow()).Returns(now ?? Now);
        return clock.Object;
    }
}
