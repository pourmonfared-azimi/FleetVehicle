#!/usr/bin/env bash
# Exports each subgraph's source schema (schema.graphqls) into its project folder.
# Commit the result: it is the schema contract that CI checks for breaking changes.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

for s in Vehicles Service Parts; do
  echo "==> Exporting $s"
  dotnet run --project "src/$s/FleetGraph.$s.Api" --no-launch-profile -- \
    schema export --output "$ROOT/src/$s/FleetGraph.$s.Api/schema.graphqls"
done
