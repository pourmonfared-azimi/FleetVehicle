using HotChocolate.Execution;

namespace FleetGraph.Service.IntegrationTests;

public class SchemaSnapshotTests(ServiceFactory factory) : IClassFixture<ServiceFactory>
{
    [Fact]
    public async Task Source_schema_matches_snapshot()
    {
        var schema = await factory.Services.GetSchemaAsync("Service");
        SchemaSnapshot.Match(schema.ToString()!, "service.graphql");
    }
}
