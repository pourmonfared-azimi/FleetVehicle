# Type and field ownership

Every type has exactly **one owning subgraph**. Other subgraphs may reference an entity by
its key (`id`) and may *contribute* fields to it, but only the owner defines its core fields.
A field defined in more than one subgraph must be marked `@shareable` everywhere — we avoid
that unless the data is genuinely identical.

| Type | Owner | Key | Contributed fields (by) | Lookups |
|---|---|---|---|---|
| `Vehicle` | Vehicles | `id` | `serviceOrders` (Service) | `vehicleById` (Vehicles, public), `vehicleById` (Service, internal) |
| `VehicleStatus` | Vehicles | – | – | – |
| `ServiceOrder` | Service | `id` | – | `serviceOrderById` (public) |
| `ServiceOrderLine` | Service | – | – | – |
| `Workshop` | Service | `id` | – | `workshopById` (public) |
| `ServiceOrderStatusChangedEvent` | Service | – | – | – |
| `Part` | Parts | `id` | – | `partById` (Parts, public), `partById` (Service, internal) |
| `StockLevel` | Parts | – | – | – |
| `UserError` (interface) | all | – | – | – |
| `PageInfo`, connections | framework | – | marked `@shareable` by `AddSourceSchemaDefaults()` | – |

## Rules

- A subgraph **never calls another subgraph**. It returns a reference (`Vehicle { id }`) and
  the gateway resolves the rest through a lookup.
- Lookups return a **nullable** type, so an unknown key gives `null` instead of failing the
  whole response.
- Internal lookups (`@internal`) exist only so the gateway can "enter" a subgraph's stub type;
  clients never see them.
- Changing the owner of a field is done with `@override` in a planned, two-step migration.
