using FleetGraph.ServiceDefaults;
using Microsoft.Extensions.Options;

namespace FleetGraph.Gateway;

/// <summary>
/// <c>POST /dev/token?role=FleetManager&amp;role=Warehouse</c> returns a signed JWT for local
/// testing. Only mapped when <c>Jwt:EnableDevTokenEndpoint</c> is true.
/// </summary>
internal static class DevTokenEndpoint
{
    private static readonly HashSet<string> s_knownRoles =
    [
        FleetGraphAuth.Roles.FleetManager,
        FleetGraphAuth.Roles.Technician,
        FleetGraphAuth.Roles.Warehouse
    ];

    public static IEndpointRouteBuilder MapDevTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/dev/token", (string[]? role, string? user, IOptions<JwtOptions> options) =>
            {
                var roles = (role ?? []).Where(s_knownRoles.Contains).Distinct().ToArray();
                var token = JwtTokenFactory.CreateToken(options.Value, user ?? "demo-user", roles);
                return Results.Ok(new { access_token = token, token_type = "Bearer", roles });
            })
            .WithName("DevToken")
            .AllowAnonymous();

        return app;
    }
}
