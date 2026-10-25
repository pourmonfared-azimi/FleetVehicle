# ADR 0002 — Hot Chocolate Fusion as the gateway

**Status:** accepted

## Context
All subgraphs are .NET. We need a gateway that supports entity resolution, subscriptions,
request batching to subgraphs, OpenTelemetry and ASP.NET Core security.

## Decision
Use **Hot Chocolate Fusion 16** (Composite Schemas spec): subgraphs annotate lookups with
`[Lookup]` / `[Internal]`, schemas are exported with `schema export`, composed with the Nitro
CLI into `gateway.far`, and served by an ASP.NET Core gateway (`AddGraphQLGateway`).

## Why
- Same stack and programming model in gateway and subgraphs; the gateway is a normal
  ASP.NET Core app (middleware, DI, header propagation, JWT, health checks).
- Follows the GraphQL Foundation's Composite Schemas specification rather than a vendor
  specific directive set.
- Variable and request batching between gateway and subgraphs; DataLoaders inside subgraphs.
- Subgraph subscriptions over SSE, or broker backed event streams for scale.

## When Apollo Router would be the choice
- Polyglot subgraphs (Node, Go, Java) that already speak Apollo Federation v2.
- An organisation already standardised on GraphOS (schema registry, usage reporting,
  contracts) or that needs Rust-level gateway throughput with a managed control plane.
Fusion 16 can also consume Apollo Federation subgraphs, so the choice is reversible at the
subgraph level.
