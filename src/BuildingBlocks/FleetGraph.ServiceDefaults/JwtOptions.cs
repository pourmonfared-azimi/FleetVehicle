namespace FleetGraph.ServiceDefaults;

/// <summary>
/// Symmetric-key JWT settings. Good enough for a self-contained demo; in production the
/// gateway and subgraphs would trust an identity provider (Entra ID, Keycloak, ...) via
/// <c>Authority</c> + JWKS instead of a shared key.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "fleetgraph-dev";

    public string Audience { get; set; } = "fleetgraph";

    /// <summary>At least 32 characters (HS256).</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Exposes <c>POST /dev/token</c> on the gateway. Never enable in production.</summary>
    public bool EnableDevTokenEndpoint { get; set; }
}
