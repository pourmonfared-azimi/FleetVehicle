# Schema design guidelines

Short and opinionated. The shared conventions are enforced in code by
`FleetGraph.GraphQL/SubgraphDefaults.cs`, so new subgraphs get them for free.

## Naming
- Types: `PascalCase` nouns from the domain (`ServiceOrder`, not `ServiceOrderDto`).
- Fields and arguments: `camelCase`. Booleans read as questions: `needsReorder`, `onlyOpen`.
- Enums: `SCREAMING_SNAKE_CASE` values (`IN_PROGRESS`).
- Mutations: `verbNoun` (`createServiceOrder`, `updateStock`), one input argument named
  `input`, a payload named `<Mutation>Payload` (mutation conventions do this).
- Subscriptions: past-tense events (`serviceOrderStatusChanged`).

## IDs and entities
- Every entity has `id` and a public `<type>ById(id)` lookup in its owning subgraph.
- The id is the federation key. Never expose raw foreign keys (`vehicleId`); expose the
  relation (`vehicle: Vehicle`) and let the gateway resolve it.

## Nullability
- Non-null by default for fields the owner always has.
- Nullable for anything that crosses a subgraph boundary or can be missing:
  lookups, references, fields behind authorization (`ownerEmail`).
- Mutation payload data fields are nullable (they are `null` when `errors` is set).

## Lists and pagination
- Top-level collections use **cursor connections** (`first/after`, `totalCount`, `pageInfo`).
  Default page size 10, max 50.
- Nested lists that are naturally small are plain lists with a `@listSize(assumedSize:)`
  annotation, so cost analysis at the gateway stays bounded.
- Filtering and sorting are explicit contracts. When a type has sensitive fields, use an
  explicit `FilterInputType` that opts fields in (see `VehicleFilterInputType`).

## Errors
- **Expected** business errors are part of the schema: each mutation has an `errors` union
  of types implementing `UserError { message code }`, plus error specific fields.
- Domain exceptions are mapped by `CreateErrorFrom` factories; internal exception messages
  never leak. Unexpected failures stay GraphQL errors (`errors` at the top level).
- Error codes are stable `SCREAMING_SNAKE_CASE` strings (`INSUFFICIENT_STOCK`).

## Descriptions
- Every type and every non-obvious field has a description; mention the owning subgraph.

## Security
- Authorization lives next to the data, in the owning subgraph (`[Authorize(Policy = ...)]`).
- Personal data is nullable, policy protected and excluded from filters.
- Depth, cost and introspection limits are on in every subgraph and at the gateway.
