using FleetGraph.Service.Api;
using FleetGraph.Service.Application.Abstractions;
using OpenTelemetry.Trace;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(
    serviceName: "service-subgraph",
    configureTracing: tracing => tracing.AddHotChocolateInstrumentation());

builder.Services
    .AddServiceApplication()
    .AddServiceInfrastructure(builder.Configuration)
    .AddScoped<IServiceOrderEventPublisher, SubscriptionEventPublisher>();

var graphql = builder
    .AddGraphQL("Service")
    .AddFleetGraphSubgraphDefaults(builder.Environment, builder.Configuration)
    .AddServiceTypes();

// Redis lets several instances of this subgraph share subscription events.
var redis = builder.Configuration.GetConnectionString("Redis");
if (string.IsNullOrWhiteSpace(redis))
{
    graphql.AddInMemorySubscriptions();
}
else
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redis));
    graphql.AddRedisSubscriptions();
}

var app = builder.Build();

app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapGraphQL();

return await app.RunWithGraphQLCommandsAsync(args);
