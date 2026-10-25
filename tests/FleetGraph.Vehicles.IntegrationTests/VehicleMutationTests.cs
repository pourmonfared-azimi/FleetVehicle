namespace FleetGraph.Vehicles.IntegrationTests;

/// <summary>Mutations get their own factory (and database) so query tests stay deterministic.</summary>
public class VehicleMutationTests(VehiclesFactory factory) : IClassFixture<VehiclesFactory>
{
    private const string RegisterVehicle = """
        mutation Register($input: RegisterVehicleInput!) {
          registerVehicle(input: $input) {
            vehicle { id vin registrationNumber status }
            errors {
              __typename
              ... on UserError { code message }
              ... on DuplicateVinError { vin }
              ... on VehicleValidationError { field }
            }
          }
        }
        """;

    private readonly HttpClient _client = factory.CreateClient();

    private static object Input(string vin) => new
    {
        input = new
        {
            vin,
            registrationNumber = "TST001",
            manufacturer = "Volvo",
            model = "FH Aero",
            modelYear = 2025,
            mileageKm = 12,
            ownerEmail = "new.owner@example.com"
        }
    };

    [Fact]
    public async Task RegisterVehicle_returns_the_new_vehicle()
    {
        var response = await _client.PostAsync(RegisterVehicle, Input("YV1LZ5947W2000100"), TestTokens.FleetManager);

        Assert.False(response.HasErrors, response.Body);
        Assert.Equal(System.Text.Json.JsonValueKind.Null, response.Get("registerVehicle", "errors").ValueKind);
        Assert.Equal("YV1LZ5947W2000100", response.Get("registerVehicle", "vehicle", "vin").GetString());
        Assert.Equal("ACTIVE", response.Get("registerVehicle", "vehicle", "status").GetString());
    }

    [Fact]
    public async Task RegisterVehicle_returns_a_typed_error_for_a_duplicate_vin()
    {
        // VIN of seeded vehicle 1.
        var response = await _client.PostAsync(RegisterVehicle, Input("YV1LZ5947W2000001"), TestTokens.FleetManager);

        Assert.False(response.HasErrors, response.Body);
        var error = response.Get("registerVehicle", "errors", 0);
        Assert.Equal("DuplicateVinError", error.GetProperty("__typename").GetString());
        Assert.Equal("DUPLICATE_VIN", error.GetProperty("code").GetString());
        Assert.Equal("YV1LZ5947W2000001", error.GetProperty("vin").GetString());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, response.Get("registerVehicle", "vehicle").ValueKind);
    }

    [Fact]
    public async Task RegisterVehicle_returns_a_validation_error_with_the_field()
    {
        var response = await _client.PostAsync(RegisterVehicle, Input("NOT-A-VIN"), TestTokens.FleetManager);

        var error = response.Get("registerVehicle", "errors", 0);
        Assert.Equal("VehicleValidationError", error.GetProperty("__typename").GetString());
        Assert.Equal("vin", error.GetProperty("field").GetString());
    }

    [Fact]
    public async Task RegisterVehicle_requires_authentication()
    {
        var response = await _client.PostAsync(RegisterVehicle, Input("YV1LZ5947W2000101"));

        Assert.Contains("AUTH_NOT_AUTHENTICATED", response.ErrorCodes);
    }

    [Fact]
    public async Task ChangeVehicleStatus_rejects_leaving_retired()
    {
        const string mutation = """
            mutation Change($id: Int!, $status: VehicleStatus!) {
              changeVehicleStatus(input: { vehicleId: $id, status: $status }) {
                vehicle { status }
                errors {
                  __typename
                  ... on UserError { code }
                  ... on InvalidVehicleStatusTransitionError { from to }
                }
              }
            }
            """;

        var retire = await _client.PostAsync(mutation, new { id = 8, status = "RETIRED" }, TestTokens.FleetManager);
        Assert.Equal("RETIRED", retire.Get("changeVehicleStatus", "vehicle", "status").GetString());

        var reactivate = await _client.PostAsync(mutation, new { id = 8, status = "ACTIVE" }, TestTokens.FleetManager);
        var error = reactivate.Get("changeVehicleStatus", "errors", 0);

        Assert.Equal("InvalidVehicleStatusTransitionError", error.GetProperty("__typename").GetString());
        Assert.Equal("RETIRED", error.GetProperty("from").GetString());
        Assert.Equal("ACTIVE", error.GetProperty("to").GetString());
    }

    [Fact]
    public async Task ChangeVehicleStatus_returns_not_found_for_unknown_vehicle()
    {
        var response = await _client.PostAsync(
            """
            mutation {
              changeVehicleStatus(input: { vehicleId: 4040, status: IN_SERVICE }) {
                errors { __typename ... on VehicleNotFoundError { vehicleId code } }
              }
            }
            """,
            token: TestTokens.Technician);

        var error = response.Get("changeVehicleStatus", "errors", 0);
        Assert.Equal("VehicleNotFoundError", error.GetProperty("__typename").GetString());
        Assert.Equal(4040, error.GetProperty("vehicleId").GetInt32());
        Assert.Equal("VEHICLE_NOT_FOUND", error.GetProperty("code").GetString());
    }
}
