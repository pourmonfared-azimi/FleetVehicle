using FleetGraph.ServiceDefaults;

namespace FleetGraph.Testing;

/// <summary>Creates tokens signed with the same development key the services use.</summary>
public static class TestTokens
{
    public static readonly JwtOptions Options = new()
    {
        Issuer = "fleetgraph-dev",
        Audience = "fleetgraph",
        SigningKey = "fleetgraph-dev-signing-key-change-me-0123456789"
    };

    public static string For(params string[] roles)
        => JwtTokenFactory.CreateToken(Options, "test-user", roles, TimeSpan.FromMinutes(10));

    public static string FleetManager => For(FleetGraphAuth.Roles.FleetManager);

    public static string Technician => For(FleetGraphAuth.Roles.Technician);

    public static string Warehouse => For(FleetGraphAuth.Roles.Warehouse);
}
