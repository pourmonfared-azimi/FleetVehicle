namespace FleetGraph.EndToEndTests;

/// <summary>A fact that only runs when FLEETGRAPH_GATEWAY_URL points to a running gateway.</summary>
public sealed class GatewayFactAttribute : FactAttribute
{
    public const string Variable = "FLEETGRAPH_GATEWAY_URL";

    public GatewayFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variable)))
        {
            Skip = $"Set {Variable} (e.g. http://localhost:5100) to run end-to-end tests.";
        }
    }

    public static HttpClient CreateClient()
        => new() { BaseAddress = new Uri(Environment.GetEnvironmentVariable(Variable)!) };
}
