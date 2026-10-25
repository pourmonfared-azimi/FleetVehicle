# ADR 0003 — Versioning and deprecation policy

**Status:** accepted

## Decision
The graph is **versionless**. There is no `/v2` endpoint and no versioned type names.

1. **Additive changes are free**: new types, new fields, new optional arguments, new enum
   values (clients must handle unknown enum values).
2. **Breaking changes go through deprecation**:
   - mark the field `@deprecated(reason: "Use X instead. Sunset: 2027-03-31.")`
     (`[GraphQLDeprecated("...")]` in C#),
   - announce it, and track usage per client (trusted document ids / client name header),
   - remove it only after the sunset date **and** when usage shows no clients.
3. **CI enforces it**: GraphQL Inspector compares each subgraph's exported schema with the
   base branch. A breaking change fails the PR unless it carries the
   `breaking-change-approved` label. Removing a field that was already deprecated is treated
   as non-breaking (`suppressRemovalOfDeprecatedField`).
4. **Composition is checked on every PR**; a change that breaks the supergraph never merges.
5. Schema snapshot tests in each subgraph catch accidental schema changes locally.

## Consequences
- Clients upgrade at their own pace; the server carries deprecated fields for a while.
- Ownership changes between subgraphs use `@override`, never delete-and-recreate.
- Nullability changes are breaking in one direction (nullable → non-null on inputs,
  non-null → nullable on outputs) and follow the same process.
