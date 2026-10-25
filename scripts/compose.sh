#!/usr/bin/env bash
# Exports the subgraph schemas and composes the Fusion supergraph (gateway.far).
#   ./scripts/compose.sh              -> Development (subgraphs on localhost:5101-5103)
#   ./scripts/compose.sh Docker       -> Docker (subgraphs on compose service names)
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
ENVIRONMENT="${1:-Development}"
ARCHIVE="src/Gateway/FleetGraph.Gateway/gateway.far"
cd "$ROOT"

dotnet tool restore
"$ROOT/scripts/export-schemas.sh"

echo "==> Composing supergraph ($ENVIRONMENT)"
dotnet nitro fusion compose \
  --source-schema-file src/Vehicles/FleetGraph.Vehicles.Api/schema.graphqls \
  --source-schema-file src/Service/FleetGraph.Service.Api/schema.graphqls \
  --source-schema-file src/Parts/FleetGraph.Parts.Api/schema.graphqls \
  --archive "$ARCHIVE" \
  --environment "$ENVIRONMENT"

# Lists without @listSize are assumed to hold 25 items for cost analysis at the gateway.
dotnet nitro fusion settings set default-list-size 25 --archive "$ARCHIVE" --environment "$ENVIRONMENT"

echo "==> Wrote $ARCHIVE"
