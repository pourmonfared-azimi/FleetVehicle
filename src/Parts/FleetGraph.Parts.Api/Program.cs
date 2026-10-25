using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(
    serviceName: "parts-subgraph",
    configureTracing: tracing => tracing.AddHotChocolateInstrumentation());

builder.Services
    .AddPartsApplication()
    .AddPartsInfrastructure(builder.Configuration);

builder
    .AddGraphQL("Parts")
    .AddFleetGraphSubgraphDefaults(builder.Environment, builder.Configuration)
    .AddPartsTypes();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapGraphQL();

return await app.RunWithGraphQLCommandsAsync(args);
