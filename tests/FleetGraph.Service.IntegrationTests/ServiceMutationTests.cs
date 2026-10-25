namespace FleetGraph.Service.IntegrationTests;

public class ServiceMutationTests(ServiceFactory factory) : IClassFixture<ServiceFactory>
{
    private const string CreateServiceOrder = """
        mutation Create($input: CreateServiceOrderInput!) {
          createServiceOrder(input: $input) {
            serviceOrder { id status vehicle { id } lines { quantity part { id } } }
            errors {
              __typename
              ... on UserError { code message }
              ... on WorkshopNotFoundError { workshopId }
              ... on ServiceOrderValidationError { field }
            }
          }
        }
        """;

    private const string UpdateStatus = """
        mutation Update($id: Int!, $status: ServiceOrderStatus!) {
          updateServiceOrderStatus(input: { serviceOrderId: $id, status: $status }) {
            serviceOrder { id status completedAt }
            errors {
              __typename
              ... on UserError { code }
              ... on InvalidServiceOrderStatusTransitionError { from to }
            }
          }
        }
        """;

    private readonly HttpClient _client = factory.CreateClient();

    private static object Input(int workshopId, params (int PartId, int Quantity)[] lines) => new
    {
        input = new
        {
            vehicleId = 3,
            workshopId,
            description = "Gearbox noise",
            scheduledAt = DateTimeOffset.UtcNow.AddDays(3),
            lines = lines.Select(l => new { partId = l.PartId, quantity = l.Quantity })
        }
    };

    [Fact]
    public async Task CreateServiceOrder_creates_a_scheduled_order()
    {
        var response = await _client.PostAsync(CreateServiceOrder, Input(2, (4, 1), (2, 6)), TestTokens.Technician);

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal("SCHEDULED", response.Get("createServiceOrder", "serviceOrder", "status").GetString());
        Assert.Equal(3, response.Get("createServiceOrder", "serviceOrder", "vehicle", "id").GetInt32());
        Assert.Equal(2, response.Get("createServiceOrder", "serviceOrder", "lines").GetArrayLength());
    }

    [Fact]
    public async Task CreateServiceOrder_returns_typed_error_for_unknown_workshop()
    {
        var response = await _client.PostAsync(CreateServiceOrder, Input(77, (1, 1)), TestTokens.Technician);

        var error = response.Get("createServiceOrder", "errors", 0);
        Assert.Equal("WorkshopNotFoundError", error.GetProperty("__typename").GetString());
        Assert.Equal(77, error.GetProperty("workshopId").GetInt32());
        Assert.Equal("WORKSHOP_NOT_FOUND", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task CreateServiceOrder_returns_validation_error_for_duplicate_parts()
    {
        var response = await _client.PostAsync(CreateServiceOrder, Input(1, (1, 1), (1, 2)), TestTokens.Technician);

        var error = response.Get("createServiceOrder", "errors", 0);
        Assert.Equal("ServiceOrderValidationError", error.GetProperty("__typename").GetString());
        Assert.Equal("lines", error.GetProperty("field").GetString());
    }

    [Fact]
    public async Task CreateServiceOrder_requires_a_role()
    {
        var response = await _client.PostAsync(CreateServiceOrder, Input(1, (1, 1)), TestTokens.Warehouse);

        Assert.Contains("AUTH_NOT_AUTHORIZED", response.ErrorCodes);
    }

    [Fact]
    public async Task UpdateServiceOrderStatus_rejects_reopening_a_completed_order()
    {
        // Seeded order 5 is completed.
        var response = await _client.PostAsync(UpdateStatus, new { id = 5, status = "IN_PROGRESS" }, TestTokens.Technician);

        var error = response.Get("updateServiceOrderStatus", "errors", 0);
        Assert.Equal("InvalidServiceOrderStatusTransitionError", error.GetProperty("__typename").GetString());
        Assert.Equal("COMPLETED", error.GetProperty("from").GetString());
    }

    [Fact]
    public async Task UpdateServiceOrderStatus_completes_an_order()
    {
        // Seeded order 1 is in progress.
        var response = await _client.PostAsync(UpdateStatus, new { id = 1, status = "COMPLETED" }, TestTokens.Technician);

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal("COMPLETED", response.Get("updateServiceOrderStatus", "serviceOrder", "status").GetString());
        Assert.NotEqual(
            System.Text.Json.JsonValueKind.Null,
            response.Get("updateServiceOrderStatus", "serviceOrder", "completedAt").ValueKind);
    }
}
