#!/usr/bin/env bash
# Runs a few operations against a running gateway (docker compose up / run-local).
set -euo pipefail
GATEWAY="${GATEWAY_URL:-http://localhost:5100}"

gql() {
  curl -sS "$GATEWAY/graphql" \
    -H 'Content-Type: application/json' \
    -H 'Accept: application/graphql-response+json' \
    ${TOKEN:+-H "Authorization: Bearer $TOKEN"} \
    -d "$1"
  echo
}

echo "==> Cross-subgraph query (Vehicles -> Service -> Parts)"
gql '{"query":"{ vehicles(first: 2, order: [{ id: ASC }]) { nodes { registrationNumber model serviceOrders(onlyOpen: true) { description status workshop { name } lines { quantity part { name stockLevel { quantityOnHand needsReorder } } } } } } }"}'

echo "==> Field authorization without token (ownerEmail -> error)"
gql '{"query":"{ vehicleById(id: 1) { registrationNumber ownerEmail } }"}'

echo "==> Getting a dev token (FleetManager + Technician + Warehouse)"
TOKEN=$(curl -sS -X POST "$GATEWAY/dev/token?role=FleetManager&role=Technician&role=Warehouse" | sed -E 's/.*"access_token":"([^"]+)".*/\1/')

echo "==> Field authorization with token"
gql '{"query":"{ vehicleById(id: 1) { registrationNumber ownerEmail } }"}'

echo "==> Mutation with typed errors (insufficient stock)"
gql '{"query":"mutation { updateStock(input: { partId: 4, quantityChange: -5 }) { part { name stockLevel { quantityOnHand } } errors { __typename ... on UserError { code message } ... on InsufficientStockError { quantityOnHand requested } } } }"}'

echo "==> Cross-subgraph mutation result"
gql '{"query":"mutation { updateServiceOrderStatus(input: { serviceOrderId: 2, status: IN_PROGRESS }) { serviceOrder { id status vehicle { registrationNumber manufacturer } } errors { __typename ... on UserError { code message } } } }"}'
