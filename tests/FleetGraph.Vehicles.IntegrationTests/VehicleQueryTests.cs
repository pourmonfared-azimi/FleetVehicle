namespace FleetGraph.Vehicles.IntegrationTests;

/// <summary>Read-only tests against the seeded data (8 vehicles).</summary>
public class VehicleQueryTests(VehiclesFactory factory) : IClassFixture<VehiclesFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Cursor_pagination_walks_through_the_vehicles()
    {
        const string query = """
            query Page($after: String) {
              vehicles(first: 3, after: $after) {
                totalCount
                pageInfo { hasNextPage endCursor }
                nodes { id }
              }
            }
            """;

        var page1 = await _client.PostAsync(query);
        Assert.False(page1.HasErrors, page1.Body);
        Assert.Equal(8, page1.Get("vehicles", "totalCount").GetInt32());
        Assert.True(page1.Get("vehicles", "pageInfo", "hasNextPage").GetBoolean());
        Assert.Equal(3, page1.Get("vehicles", "nodes").GetArrayLength());

        var cursor = page1.Get("vehicles", "pageInfo", "endCursor").GetString();
        var page2 = await _client.PostAsync(query, new { after = cursor });

        Assert.False(page2.HasErrors, page2.Body);
        Assert.Equal(4, page2.Get("vehicles", "nodes", 0, "id").GetInt32());
    }

    [Fact]
    public async Task Filtering_and_sorting_are_pushed_to_the_query()
    {
        var response = await _client.PostAsync("""
            {
              vehicles(
                where: { manufacturer: { eq: "Volvo" } }
                order: [{ modelYear: DESC }]
              ) {
                nodes { manufacturer modelYear }
              }
            }
            """);

        Assert.False(response.HasErrors, response.Body);
        var nodes = response.Get("vehicles", "nodes").EnumerateArray().ToList();

        Assert.Equal(4, nodes.Count);
        Assert.All(nodes, n => Assert.Equal("Volvo", n.GetProperty("manufacturer").GetString()));
        var years = nodes.Select(n => n.GetProperty("modelYear").GetInt32()).ToList();
        Assert.Equal(years.OrderByDescending(y => y), years);
    }

    [Fact]
    public async Task Personal_data_is_not_filterable()
    {
        var response = await _client.PostAsync("""
            { vehicles(where: { ownerEmail: { eq: "fleet.north@example.com" } }) { nodes { id } } }
            """);

        Assert.True(response.HasErrors, "ownerEmail must not be part of VehicleFilterInput.");
    }

    [Fact]
    public async Task Lookup_by_id_returns_the_entity()
    {
        var response = await _client.PostAsync("{ vehicleById(id: 3) { id vin manufacturer status } }");

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal("Scania", response.Get("vehicleById", "manufacturer").GetString());
        Assert.Equal("ACTIVE", response.Get("vehicleById", "status").GetString());
    }

    [Fact]
    public async Task Lookup_returns_null_for_unknown_id()
    {
        var response = await _client.PostAsync("{ vehicleById(id: 9999) { id } }");

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal(System.Text.Json.JsonValueKind.Null, response.Get("vehicleById").ValueKind);
    }

    [Fact]
    public async Task Owner_email_requires_authentication()
    {
        var response = await _client.PostAsync("{ vehicleById(id: 1) { registrationNumber ownerEmail } }");

        Assert.Contains("AUTH_NOT_AUTHENTICATED", response.ErrorCodes);
        Assert.Equal("ABC123", response.Get("vehicleById", "registrationNumber").GetString());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, response.Get("vehicleById", "ownerEmail").ValueKind);
    }

    [Fact]
    public async Task Owner_email_requires_the_fleet_manager_role()
    {
        var response = await _client.PostAsync(
            "{ vehicleById(id: 1) { ownerEmail } }",
            token: TestTokens.Technician);

        Assert.Contains("AUTH_NOT_AUTHORIZED", response.ErrorCodes);
    }

    [Fact]
    public async Task Fleet_manager_can_read_owner_email()
    {
        var response = await _client.PostAsync(
            "{ vehicleById(id: 1) { ownerEmail } }",
            token: TestTokens.FleetManager);

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal("fleet.north@example.com", response.Get("vehicleById", "ownerEmail").GetString());
    }
}
