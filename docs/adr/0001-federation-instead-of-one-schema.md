# ADR 0001 — Federation instead of one big schema

**Status:** accepted

## Context
Vehicles, workshop service and spare parts are owned by different teams, change at a
different pace and have different load profiles (the parts catalog is read heavily, service
orders are write heavy around workshop hours). Clients — a fleet dashboard, a workshop app —
want one API and one round trip per screen.

## Options
1. **One monolithic GraphQL server** over all databases.
2. **Schema stitching** in a BFF that calls each service.
3. **Federation**: each domain is a subgraph that owns its types; a gateway composes them.

## Decision
Federation. Each subgraph owns its data and its part of the graph, references other entities
only by key, and never calls another subgraph. The gateway plans cross-subgraph queries.

## Consequences
- Teams deploy independently; the contract between them is the composed schema, checked in CI.
- Composition rules (one owner per field, explicit lookups) force clear ownership.
- Cross-subgraph consistency is eventual: e.g. the Service subgraph cannot validate that a
  vehicle id exists without coupling to Vehicles. The gateway returns `null` for unknown
  references; if strict validation becomes necessary it should come from an event-fed local
  copy, not a synchronous call.
- More moving parts (gateway, composition step). Worth it from ~2 teams; for one team a
  modular monolith with one schema is simpler.
