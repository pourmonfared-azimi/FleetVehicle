using HotChocolate.Execution;

namespace FleetGraph.Parts.IntegrationTests;

public class SchemaSnapshotTests(PartsFactory factory) : IClassFixture<PartsFactory>
{
    [Fact]
    public async Task Source_schema_matches_snapshot()
    {
        var schema = await factory.Services.GetSchemaAsync("Parts");
        SchemaSnapshot.Match(schema.ToString()!, "parts.graphql");
    }
}
