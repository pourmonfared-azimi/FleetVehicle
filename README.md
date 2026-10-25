# FleetGraph — a federated GraphQL supergraph in .NET 10

FleetGraph is a small but complete reference implementation of **GraphQL federation** with
[Hot Chocolate Fusion](https://chillicream.com/docs/fusion) (v16): three independently
deployable subgraphs, one gateway, schema governance in CI, typed errors, subscriptions,
DataLoaders, field-level authorization and end-to-end OpenTelemetry tracing.

```
                         ┌──────────────────────────────┐
  Client ── HTTP/WS ───► │  Gateway (Hot Chocolate      │──► Jaeger (OTLP)
  (Nitro, app, curl)     │  Fusion, gateway.far)        │
                         └──────┬─────────┬─────────┬───┘
                    HTTP+batching│   HTTP+SSE│    HTTP+batching
                         ┌───────▼──┐ ┌────▼─────┐ ┌─▼────────┐
                         │ Vehicles │ │ Service  │ │  Parts   │   subgraphs
                         │ subgraph │ │ subgraph │ │ subgraph │   (.NET 10, Clean Architecture)
                         └────┬─────┘ └──┬────┬──┘ └────┬─────┘
                              │          │    └─ Redis (subscriptions)
                              └──── PostgreSQL (one database per subgraph) ────┘
```

| Subgraph | Owns | Contributes to other types |
|---|---|---|
| **Vehicles** | `Vehicle`, `VehicleStatus` | – |
| **Service** | `ServiceOrder`, `ServiceOrderLine`, `Workshop` | `Vehicle.serviceOrders` |
| **Parts** | `Part`, `StockLevel` | – |

One query can return a vehicle, its open service orders, the workshop and the stock of every
part each order needs — the gateway plans the calls across the three subgraphs.

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`global.json` pins 10.0.100, later feature bands roll forward)
- Docker Desktop (or another Docker engine), running, for `docker compose`
- Node.js only if you want to run the breaking-change check locally (`npx @graphql-inspector/cli`)

## First run

```powershell
dotnet build FleetGraph.slnx
dotnet tool restore             # Nitro CLI (fusion compose)
./scripts/compose.ps1           # export schemas + compose gateway.far   (bash: ./scripts/compose.sh)
dotnet test                     # schema snapshots are created on the first run
docker compose up --build
```

The first run creates files that **must be committed**, because CI checks against them:

| File | Why |
|---|---|
| `src/*/FleetGraph.*.Api/schema.graphqls` | Schema contract: drift check and breaking change check |
| `tests/FleetGraph.*.IntegrationTests/__snapshots__/*` | Schema snapshots; with `CI=true` a missing snapshot fails the test |

`src/Gateway/FleetGraph.Gateway/gateway.far` is git-ignored: it is only for local runs, and
the gateway Docker image composes its own for the `Docker` environment.

## Run it

**Docker (one command):**

```bash
docker compose up --build
```

| URL | What |
|---|---|
| http://localhost:5100/graphql | Gateway + Nitro GraphQL IDE |
| http://localhost:16686 | Jaeger (traces) |
| http://localhost:5101-5103/graphql | Subgraphs (debug only; private in production) |

The gateway image build **exports the subgraph schemas and composes the supergraph**, so a
broken composition fails the build.

**Without Docker** (in-memory databases, Windows PowerShell):

```powershell
dotnet tool restore
./scripts/compose.ps1          # export schemas + compose gateway.far (localhost URLs)
./scripts/run-local.ps1        # starts the 3 subgraphs and the gateway
```

(`./scripts/compose.sh` for bash.)

## Try it

```graphql
query FleetOverview {
  vehicles(first: 3, where: { manufacturer: { eq: "Volvo" } }, order: [{ modelYear: DESC }]) {
    totalCount
    pageInfo { hasNextPage endCursor }
    nodes {
      registrationNumber          # Vehicles subgraph
      model
      serviceOrders(onlyOpen: true) {   # Service subgraph
        description
        status
        workshop { name city }
        lines {
          quantity
          part {                  # Parts subgraph
            name
            stockLevel { quantityOnHand needsReorder }
          }
        }
      }
    }
  }
}
```

Mutations return **typed errors** (a union per mutation, all implementing `UserError`):

```graphql
mutation {
  updateStock(input: { partId: 4, quantityChange: -5 }) {
    part { name stockLevel { quantityOnHand } }
    errors {
      __typename
      ... on UserError { code message }
      ... on InsufficientStockError { quantityOnHand requested }
    }
  }
}
```

Mutations and personal data require a token. In the demo the gateway issues one:

```bash
curl -X POST "http://localhost:5100/dev/token?role=FleetManager&role=Technician&role=Warehouse"
# then send:  Authorization: Bearer <access_token>
```

Subscribe (WebSocket or SSE) and change a status in another tab:

```graphql
subscription {
  serviceOrderStatusChanged {
    previousStatus newStatus changedAt
    serviceOrder { description }
    vehicle { registrationNumber }   # resolved from the Vehicles subgraph per event
  }
}
```

```graphql
mutation { updateServiceOrderStatus(input: { serviceOrderId: 2, status: IN_PROGRESS }) { serviceOrder { status } } }
```

`./scripts/smoke-test.sh` runs these against a running stack.

## What it demonstrates

| Area | Where |
|---|---|
| Federation: entities, public + `@internal` lookups, entity stubs | `src/*/Api/Types/*Queries.cs`, `src/Service/.../Types/Nodes.cs` |
| Queries: filtering, sorting, cursor pagination (pushed to SQL via `IQueryable`) | `VehicleQueries.cs`, explicit `VehicleFilterInputType` |
| Mutations: mutation conventions, typed errors via `CreateErrorFrom` factories | `*Mutations.cs`, `*Errors.cs` |
| Subscriptions: subgraph subscription (Redis / in-memory), consumed by the gateway over SSE | `ServiceSubscriptions.cs` |
| DataLoaders (source generated) – no N+1 across nested federated queries | `*DataLoaders.cs` |
| Security: JWT at the edge, header propagation, field level `[Authorize]`, depth + cost limits, introspection off outside development, trusted documents | `Gateway/Program.cs`, `ServiceDefaults`, `GraphQL/SubgraphDefaults.cs` |
| Observability: OpenTelemetry traces + metrics in gateway and subgraphs | `ServiceDefaults`, [docs/observability.md](docs/observability.md) |
| Schema governance: exported schemas, composition check, breaking change check, snapshot tests | `.github/workflows/ci.yml`, [docs/adr/0003](docs/adr/0003-versioning-and-deprecation.md) |
| Clean Architecture per subgraph: Domain → Application → Infrastructure → Api | `src/<Subgraph>/*` |

## Solution layout

```
src/
  BuildingBlocks/
    FleetGraph.SharedKernel     Entity, DomainException, Guard (no dependencies)
    FleetGraph.ServiceDefaults  OpenTelemetry, JWT, policies, health checks
    FleetGraph.GraphQL          Subgraph conventions (paging, errors, limits, instrumentation)
  Vehicles/ Service/ Parts/     per subgraph:
    *.Domain                    aggregates + business rules (pure C#)
    *.Application               use cases (handlers), ports (repository/event interfaces)
    *.Infrastructure            EF Core (PostgreSQL / in-memory), seeding
    *.Api                       GraphQL schema (the contract), DataLoaders, Program.cs
  Gateway/FleetGraph.Gateway    Fusion gateway, trusted documents, dev token endpoint
tests/
  FleetGraph.UnitTests          Domain + Application (xUnit + Moq)
  FleetGraph.*.IntegrationTests Each subgraph over HTTP (WebApplicationFactory), schema snapshots
  FleetGraph.EndToEndTests      Through the gateway (runs when FLEETGRAPH_GATEWAY_URL is set)
docs/                           ADRs, schema guidelines, ownership, observability
```

Dependencies point inwards: `Api → Application → Domain`, `Infrastructure → Application`.
The Api project is the composition root. The GraphQL types (`VehicleNode`, …) are a separate
contract from the domain entities, so the schema can evolve without leaking domain internals.

## Tests

```bash
dotnet test                                      # unit + integration (no Docker needed)
UPDATE_SNAPSHOTS=1 dotnet test                   # accept an intended schema change
FLEETGRAPH_GATEWAY_URL=http://localhost:5100 dotnet test tests/FleetGraph.EndToEndTests
```

## Schema governance (summary)

1. Each subgraph exports its source schema: `./scripts/export-schemas.sh` → `schema.graphqls` (committed).
2. CI fails if the committed schema is out of date (drift), if composition fails, or if a
   PR contains breaking changes (GraphQL Inspector) without the `breaking-change-approved` label.
3. No `/v2`: the graph evolves additively; removals go through `@deprecated` with a sunset
   date ([ADR 0003](docs/adr/0003-versioning-and-deprecation.md)).
4. Every type has one owning subgraph ([docs/ownership.md](docs/ownership.md)); naming,
   nullability, pagination and error rules are in [docs/schema-guidelines.md](docs/schema-guidelines.md).

## Troubleshooting

| Symptom | Cause / fix |
|---|---|
| `failed to connect to the docker API at npipe:////./pipe/dockerDesktopLinuxEngine` | Docker Desktop is not running. Start it and retry `docker compose up --build`. |
| `Schema file '.../schema.graphqls' does not exist` in `compose.ps1` | Run the script from an up-to-date checkout: the export passes absolute `--output` paths (a relative path is resolved against the project folder). |
| Mutations return `AUTH_NOT_AUTHORIZED` | The token has no roles. Check the `roles` array in the `/dev/token` response and request the roles you need, e.g. `?role=Technician`. |
| `/dev/token` returns 404 | The endpoint is only mapped when `Jwt:EnableDevTokenEndpoint=true` (Development settings, and `docker-compose.yml`). |
| A schema snapshot test fails | The schema changed. If it is intended: `UPDATE_SNAPSHOTS=1 dotnet test`, run `./scripts/export-schemas.*`, and commit both. |
| Subscription returns only `:` lines | Those are SSE keep-alives; no event has been published yet. Change a service order status in another tab. |

## Tech

.NET 10 · ASP.NET Core · Hot Chocolate 16.7 / Fusion · Nitro CLI · EF Core 10 · PostgreSQL ·
Redis · OpenTelemetry · Jaeger · xUnit · Moq · Docker Compose · GitHub Actions
