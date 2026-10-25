using FleetGraph.Gateway;
using FleetGraph.ServiceDefaults;
using HotChocolate.PersistedOperations;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(
    serviceName: "gateway",
    configureTracing: tracing => tracing.AddHotChocolateFusionInstrumentation());

// Forward the caller's identity to the subgraphs: authorization decisions are made where the
// data lives (field level [Authorize] in the subgraphs), the gateway only validates the token.
builder.Services.AddHeaderPropagation(options =>
{
    options.Headers.Add("Authorization");
    options.Headers.Add("GraphQL-Preflight");
});

// "fusion" is the default HTTP client name used for every subgraph (schema-settings.json).
builder.Services
    .AddHttpClient("fusion")
    .AddHeaderPropagation();

var isDevelopment = builder.Environment.IsDevelopment();
var allowIntrospection = builder.Configuration.GetValue<bool?>("GraphQL:AllowIntrospection") ?? isDevelopment;
var enforceTrustedDocuments = builder.Configuration.GetValue<bool>("TrustedDocuments:Enforce");
var trustedDocumentsPath = Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration["TrustedDocuments:Path"] ?? "trusted-documents");
var archivePath = Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration["Fusion:ArchivePath"] ?? "gateway.far");

builder
    .AddGraphQLGateway()
    .AddFileSystemConfiguration(archivePath)
    .AddInstrumentation()
    // Request limits at the edge: depth, cost (on by default in Fusion 16.7) and introspection.
    .AddMaxExecutionDepthRule(20, skipIntrospectionFields: true)
    .DisableIntrospection(!allowIntrospection)
    .ModifyCostOptions(o =>
    {
        o.MaxFieldCost = 5_000;
        o.MaxTypeCost = 20_000;
    })
    // Trusted documents: known clients send only a document id. With Enforce=true, arbitrary
    // queries are rejected (recommended for first-party production clients).
    .ConfigureSchemaServices((_, services) =>
        services.AddSingleton<IOperationDocumentStorage>(
            new FileSystemTrustedDocumentStorage(trustedDocumentsPath)))
    .UsePersistedOperationPipeline()
    .ModifyRequestOptions(o =>
    {
        o.CollectOperationPlanTelemetry = isDevelopment;
        o.PersistedOperations.OnlyAllowPersistedDocuments = enforceTrustedDocuments;
    });

var app = builder.Build();

app.UseWebSockets();
app.UseHeaderPropagation();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

if (app.Configuration.GetValue<bool>($"{JwtOptions.SectionName}:{nameof(JwtOptions.EnableDevTokenEndpoint)}"))
{
    app.MapDevTokenEndpoint();
}

app.MapGraphQL();

app.Run();

/// <summary>Entry point marker for tests.</summary>
public partial class Program;
