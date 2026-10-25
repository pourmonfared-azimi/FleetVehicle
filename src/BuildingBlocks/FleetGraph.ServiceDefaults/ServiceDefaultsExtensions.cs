using FleetGraph.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class ServiceDefaultsExtensions
{
    /// <summary>
    /// Adds OpenTelemetry (traces + metrics), JWT bearer authentication, the shared
    /// authorization policies and a health check endpoint.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="serviceName">The service name reported to the tracing backend.</param>
    /// <param name="configureTracing">Extra instrumentation (e.g. Hot Chocolate / Fusion).</param>
    public static IHostApplicationBuilder AddServiceDefaults(
        this IHostApplicationBuilder builder,
        string serviceName,
        Action<TracerProviderBuilder>? configureTracing = null)
    {
        builder.AddFleetGraphTelemetry(serviceName, configureTracing);
        builder.AddFleetGraphAuthentication();
        builder.Services.AddHealthChecks();
        return builder;
    }

    public static IHostApplicationBuilder AddFleetGraphTelemetry(
        this IHostApplicationBuilder builder,
        string serviceName,
        Action<TracerProviderBuilder>? configureTracing = null)
    {
        var otel = builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName)
                .AddAttributes(
                [
                    new KeyValuePair<string, object>(
                        "deployment.environment",
                        builder.Environment.EnvironmentName)
                ]))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        // Health probes are noise in a trace UI.
                        o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation();

                configureTracing?.Invoke(tracing);
            })
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());

        // Export over OTLP only when an endpoint is configured (Jaeger, Aspire dashboard,
        // OpenTelemetry Collector -> Splunk, ...). Standard OTEL_* env vars apply.
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            otel.UseOtlpExporter();
        }

        return builder;
    }

    public static IHostApplicationBuilder AddFleetGraphAuthentication(
        this IHostApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(JwtOptions.SectionName);
        builder.Services.Configure<JwtOptions>(section);
        var jwt = section.Get<JwtOptions>() ?? new JwtOptions();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtTokenFactory.CreateSigningKey(jwt),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = FleetGraphAuth.NameClaimType,
                    RoleClaimType = FleetGraphAuth.RoleClaimType
                };
            });

        builder.Services
            .AddAuthorizationBuilder()
            .AddPolicy(FleetGraphAuth.Policies.CanViewOwnerData, p => p
                .RequireAuthenticatedUser()
                .RequireRole(FleetGraphAuth.Roles.FleetManager))
            .AddPolicy(FleetGraphAuth.Policies.CanManageStock, p => p
                .RequireAuthenticatedUser()
                .RequireRole(FleetGraphAuth.Roles.Warehouse))
            .AddPolicy(FleetGraphAuth.Policies.CanManageServiceOrders, p => p
                .RequireAuthenticatedUser()
                .RequireRole(FleetGraphAuth.Roles.Technician, FleetGraphAuth.Roles.FleetManager));

        return builder;
    }

    public static IEndpointRouteBuilder MapDefaultEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}
