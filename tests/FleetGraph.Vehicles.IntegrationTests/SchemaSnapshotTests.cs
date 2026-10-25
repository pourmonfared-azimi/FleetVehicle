using HotChocolate.Execution;

namespace FleetGraph.Vehicles.IntegrationTests;

public class SchemaSnapshotTests(VehiclesFactory factory) : IClassFixture<VehiclesFactory>
{
    [Fact]
    public async Task Source_schema_matches_snapshot()
    {
        var schema = await factory.Services.GetSchemaAsync("Vehicles");
        SchemaSnapshot.Match(schema.ToString()!, "vehicles.graphql");
    }
}
