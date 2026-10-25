namespace FleetGraph.Service.IntegrationTests;

public class ServiceQueryTests(ServiceFactory factory) : IClassFixture<ServiceFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Service_orders_can_be_filtered_by_status()
    {
        var response = await _client.PostAsync("""
            { serviceOrders(where: { status: { eq: AWAITING_PARTS } }) { nodes { description status } } }
            """);

        Assert.False(response.HasErrors, response.Body);
        var node = Assert.Single(response.Get("serviceOrders", "nodes").EnumerateArray());
        Assert.Equal("Clutch replacement", node.GetProperty("description").GetString());
    }

    /// <summary>
    /// This is exactly what the Fusion gateway does to resolve <c>Vehicle.serviceOrders</c>:
    /// it calls the internal <c>vehicleById</c> lookup with keys it got from the Vehicles subgraph.
    /// </summary>
    [Fact]
    public async Task Vehicle_entity_stub_contributes_service_orders()
    {
        var response = await _client.PostAsync("""
            {
              a: vehicleById(id: 1) { id serviceOrders { description } }
              b: vehicleById(id: 2) { id open: serviceOrders(onlyOpen: true) { description status } }
            }
            """);

        Assert.False(response.HasErrors, response.Body);

        var vehicle1 = response.Get("a", "serviceOrders").EnumerateArray()
            .Select(o => o.GetProperty("description").GetString()).ToList();
        Assert.Equal(new[] { "Replace front brake pads", "Annual inspection and oil change" }, vehicle1);

        Assert.All(
            response.Get("b", "open").EnumerateArray(),
            o => Assert.NotEqual("COMPLETED", o.GetProperty("status").GetString()));
    }

    [Fact]
    public async Task Service_order_references_vehicle_parts_and_workshop()
    {
        var response = await _client.PostAsync("""
            {
              serviceOrderById(id: 1) {
                vehicle { id }
                workshop { name city }
                lines { quantity part { id } }
              }
            }
            """);

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal(1, response.Get("serviceOrderById", "vehicle", "id").GetInt32());
        Assert.Equal("Eskilstuna", response.Get("serviceOrderById", "workshop", "city").GetString());
        Assert.Equal(2, response.Get("serviceOrderById", "lines").GetArrayLength());
        Assert.Equal(1, response.Get("serviceOrderById", "lines", 0, "part", "id").GetInt32());
    }

    [Fact]
    public async Task Workshops_are_paginated()
    {
        var response = await _client.PostAsync("{ workshops(first: 2) { totalCount nodes { name } } }");

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal(3, response.Get("workshops", "totalCount").GetInt32());
        Assert.Equal(2, response.Get("workshops", "nodes").GetArrayLength());
    }
}
