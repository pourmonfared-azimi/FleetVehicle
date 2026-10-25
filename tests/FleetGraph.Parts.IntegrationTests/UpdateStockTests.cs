namespace FleetGraph.Parts.IntegrationTests;

public class UpdateStockTests(PartsFactory factory) : IClassFixture<PartsFactory>
{
    private const string UpdateStock = """
        mutation Update($partId: Int!, $change: Int!) {
          updateStock(input: { partId: $partId, quantityChange: $change }) {
            part { id stockLevel { quantityOnHand } }
            errors {
              __typename
              ... on UserError { code message }
              ... on InsufficientStockError { quantityOnHand requested }
              ... on StockValidationError { field }
              ... on PartNotFoundError { partId }
            }
          }
        }
        """;

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Warehouse_can_add_stock()
    {
        var response = await _client.PostAsync(UpdateStock, new { partId = 1, change = 5 }, TestTokens.Warehouse);

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal(47, response.Get("updateStock", "part", "stockLevel", "quantityOnHand").GetInt32());
    }

    [Fact]
    public async Task Removing_more_than_on_hand_returns_insufficient_stock()
    {
        var response = await _client.PostAsync(UpdateStock, new { partId = 4, change = -5 }, TestTokens.Warehouse);

        var error = response.Get("updateStock", "errors", 0);
        Assert.Equal("InsufficientStockError", error.GetProperty("__typename").GetString());
        Assert.Equal("INSUFFICIENT_STOCK", error.GetProperty("code").GetString());
        Assert.Equal(1, error.GetProperty("quantityOnHand").GetInt32());
        Assert.Equal(5, error.GetProperty("requested").GetInt32());
    }

    [Fact]
    public async Task Zero_change_returns_validation_error()
    {
        var response = await _client.PostAsync(UpdateStock, new { partId = 2, change = 0 }, TestTokens.Warehouse);

        var error = response.Get("updateStock", "errors", 0);
        Assert.Equal("StockValidationError", error.GetProperty("__typename").GetString());
        Assert.Equal("quantityChange", error.GetProperty("field").GetString());
    }

    [Fact]
    public async Task Unknown_part_returns_not_found()
    {
        var response = await _client.PostAsync(UpdateStock, new { partId = 999, change = 1 }, TestTokens.Warehouse);

        var error = response.Get("updateStock", "errors", 0);
        Assert.Equal("PartNotFoundError", error.GetProperty("__typename").GetString());
        Assert.Equal(999, error.GetProperty("partId").GetInt32());
    }

    [Fact]
    public async Task Only_the_warehouse_role_can_change_stock()
    {
        var response = await _client.PostAsync(UpdateStock, new { partId = 1, change = 1 }, TestTokens.Technician);

        Assert.Contains("AUTH_NOT_AUTHORIZED", response.ErrorCodes);
    }
}
