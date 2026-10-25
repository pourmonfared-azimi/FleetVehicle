using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(
    serviceName: "vehicles-subgraph",
    configureTracing: tracing => tracing.AddHotChocolateInstrumentation());

builder.Services
    .AddVehiclesApplication()
    .AddVehiclesInfrastructure(builder.Configuration);

// The name is the source schema name used by Fusion composition (schema-settings.json).
builder
    .AddGraphQL("Vehicles")
    .AddFleetGraphSubgraphDefaults(builder.Environment, builder.Configuration)
    .AddVehiclesTypes();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapGraphQL();

// Enables `dotnet run -- schema export --output schema.graphqls`.
return await app.RunWithGraphQLCommandsAsync(args);
