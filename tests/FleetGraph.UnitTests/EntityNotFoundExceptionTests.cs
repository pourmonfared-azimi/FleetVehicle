using FleetGraph.SharedKernel;

namespace FleetGraph.UnitTests;

public class EntityNotFoundExceptionTests
{
    [Theory]
    [InlineData("Vehicle", "VEHICLE_NOT_FOUND")]
    [InlineData("ServiceOrder", "SERVICE_ORDER_NOT_FOUND")]
    [InlineData("Part", "PART_NOT_FOUND")]
    public void Builds_a_stable_error_code(string entity, string expected)
        => Assert.Equal(expected, new EntityNotFoundException(entity, 1).Code);
}
