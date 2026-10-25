namespace FleetGraph.Parts.IntegrationTests;

public class PartsQueryTests(PartsFactory factory) : IClassFixture<PartsFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Parts_can_be_filtered_by_category_and_sorted_by_price()
    {
        var response = await _client.PostAsync("""
            {
              parts(where: { category: { in: [FILTERS, EXHAUST] } }, order: [{ unitPrice: DESC }]) {
                nodes { partNumber category unitPrice }
              }
            }
            """);

        Assert.False(response.HasErrors, response.Body);
        var numbers = response.Get("parts", "nodes").EnumerateArray()
            .Select(n => n.GetProperty("partNumber").GetString())
            .ToList();

        Assert.Equal(new[] { "AB-PUMP-02", "AB-NOZ-11", "AD-CART-07", "OF-2210" }, numbers);
    }

    [Fact]
    public async Task Stock_levels_are_resolved_per_part()
    {
        var response = await _client.PostAsync("""
            { parts(first: 10) { nodes { id stockLevel { quantityOnHand needsReorder } } } }
            """);

        Assert.False(response.HasErrors, response.Body);
        var turbo = response.Get("parts", "nodes").EnumerateArray().Single(n => n.GetProperty("id").GetInt32() == 10);
        Assert.Equal(0, turbo.GetProperty("stockLevel").GetProperty("quantityOnHand").GetInt32());
        Assert.True(turbo.GetProperty("stockLevel").GetProperty("needsReorder").GetBoolean());
    }

    [Fact]
    public async Task Lookups_by_id_and_part_number()
    {
        var response = await _client.PostAsync("""
            {
              byId: partById(id: 3) { name }
              byNumber: partByPartNumber(partNumber: "bp-fr-880") { id }
            }
            """);

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal("Brake pad set, front axle", response.Get("byId", "name").GetString());
        Assert.Equal(3, response.Get("byNumber", "id").GetInt32());
    }
}
