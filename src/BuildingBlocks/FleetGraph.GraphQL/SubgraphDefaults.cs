using FleetGraph.GraphQL;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class SubgraphDefaults
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;
    public const int MaxExecutionDepth = 15;

    /// <summary>
    /// Applies the FleetGraph conventions to a subgraph (a Fusion "source schema").
    /// </summary>
    public static IRequestExecutorBuilder AddFleetGraphSubgraphDefaults(
        this IRequestExecutorBuilder builder,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        // Introspection is a development tool; the schema contract lives in the exported
        // schema.graphqls files, not in runtime introspection.
        var allowIntrospection =
            configuration.GetValue<bool?>("GraphQL:AllowIntrospection")
            ?? environment.IsDevelopment();

        return builder
            // Registers this schema as a Fusion source schema: enables request/variable
            // batching for the gateway and marks framework types (PageInfo, connections)
            // as @shareable so composition across subgraphs succeeds.
            .AddSourceSchemaDefaults()
            .AddAuthorization()
            .AddFiltering()
            .AddSorting()
            .AddMutationConventions(applyToAllMutations: true)
            .AddErrorInterfaceType<IUserError>()
            .ModifyPagingOptions(o =>
            {
                o.DefaultPageSize = DefaultPageSize;
                o.MaxPageSize = MaxPageSize;
                o.IncludeTotalCount = true;
            })
            .AddMaxExecutionDepthRule(MaxExecutionDepth, skipIntrospectionFields: true)
            // Subgraphs are private and only called by the gateway, which enforces the
            // client-facing cost limits. The gateway batches entity lookups (one request,
            // many variable sets) and cost is summed across variable sets, so the subgraph
            // limit must be higher than the gateway limit.
            .ModifyCostOptions(o =>
            {
                o.MaxFieldCost = 20_000;
                o.MaxTypeCost = 100_000;
            })
            .DisableIntrospection(!allowIntrospection)
            .AddInstrumentation()
            .ModifyRequestOptions(o => o.IncludeExceptionDetails = environment.IsDevelopment());
    }
}
