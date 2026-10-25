namespace FleetGraph.EndToEndTests;

public class GatewayTests
{
    [GatewayFact]
    public async Task One_query_spans_all_three_subgraphs()
    {
        using var client = GatewayFactAttribute.CreateClient();

        var response = await client.PostAsync("""
            {
              vehicleById(id: 1) {
                registrationNumber                 # Vehicles
                serviceOrders {                    # Service
                  description
                  workshop { name }                # Service
                  lines {
                    quantity
                    part {                         # Parts
                      name
                      stockLevel { quantityOnHand }
                    }
                  }
                }
              }
            }
            """);

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal("ABC123", response.Get("vehicleById", "registrationNumber").GetString());

        var order = response.Get("vehicleById", "serviceOrders").EnumerateArray()
            .Single(o => o.GetProperty("description").GetString() == "Annual inspection and oil change");
        var part = order.GetProperty("lines")[0].GetProperty("part");

        Assert.Equal("Oil filter", part.GetProperty("name").GetString());
        Assert.True(part.GetProperty("stockLevel").GetProperty("quantityOnHand").GetInt32() >= 0);
    }

    [GatewayFact]
    public async Task Paginated_vehicles_with_nested_service_orders()
    {
        using var client = GatewayFactAttribute.CreateClient();

        var response = await client.PostAsync("""
            {
              vehicles(first: 5, where: { manufacturer: { eq: "Volvo" } }) {
                nodes { vin serviceOrders(onlyOpen: true) { status workshop { city } } }
              }
            }
            """);

        Assert.False(response.HasErrors, response.Body);
        Assert.NotEmpty(response.Get("vehicles", "nodes").EnumerateArray());
    }

    [GatewayFact]
    public async Task Authorization_header_is_forwarded_to_subgraphs()
    {
        using var client = GatewayFactAttribute.CreateClient();
        const string query = "{ vehicleById(id: 1) { ownerEmail } }";

        var anonymous = await client.PostAsync(query);
        var manager = await client.PostAsync(query, token: TestTokens.FleetManager);

        Assert.True(anonymous.HasErrors);
        Assert.False(manager.HasErrors, manager.Body);
        Assert.Equal("fleet.north@example.com", manager.Get("vehicleById", "ownerEmail").GetString());
    }

    [GatewayFact]
    public async Task Mutation_result_is_joined_across_subgraphs()
    {
        using var client = GatewayFactAttribute.CreateClient();

        var response = await client.PostAsync(
            """
            mutation {
              createServiceOrder(input: {
                vehicleId: 2, workshopId: 1, description: "E2E brake check",
                scheduledAt: "2030-01-01T08:00:00Z", lines: [{ partId: 3, quantity: 1 }]
              }) {
                serviceOrder {
                  status
                  vehicle { registrationNumber }   # resolved by the Vehicles subgraph
                  lines { part { partNumber } }    # resolved by the Parts subgraph
                }
                errors { __typename }
              }
            }
            """,
            token: TestTokens.Technician);

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal("DEF456", response.Get("createServiceOrder", "serviceOrder", "vehicle", "registrationNumber").GetString());
        Assert.Equal("BP-FR-880", response.Get("createServiceOrder", "serviceOrder", "lines", 0, "part", "partNumber").GetString());
    }

    [GatewayFact]
    public async Task Trusted_document_can_be_executed_by_id()
    {
        using var client = GatewayFactAttribute.CreateClient();

        var response = await client.PostDocumentIdAsync(TrustedDocuments.VehicleOverview, new { first = 2 });

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal(2, response.Get("vehicles", "nodes").GetArrayLength());
    }
}
