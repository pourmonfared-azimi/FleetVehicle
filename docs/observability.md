# Observability

Every service (gateway and the three subgraphs) uses OpenTelemetry via
`FleetGraph.ServiceDefaults`:

- **Traces**: ASP.NET Core + HttpClient instrumentation, plus Hot Chocolate
  (`AddHotChocolateInstrumentation`) in the subgraphs and Fusion
  (`AddHotChocolateFusionInstrumentation`) in the gateway. One client query produces one
  trace: gateway request → operation planning → one span per subgraph call → resolver and
  DataLoader spans inside each subgraph.
- **Metrics**: ASP.NET Core and HttpClient meters.
- **Export**: OTLP when `OTEL_EXPORTER_OTLP_ENDPOINT` is set (Docker Compose points it to Jaeger).

Open http://localhost:16686, pick the `gateway` service and run a nested query to see the
spans fan out to `vehicles-subgraph`, `service-subgraph` and `parts-subgraph`. The DataLoader
spans show that a list of service orders with 20 parts results in one stock query, not 20.

## Exporting to Splunk

Nothing in the services changes. Point the OTLP exporter at an OpenTelemetry Collector and
let the collector forward to Splunk:

```yaml
# otel-collector.yaml
receivers:
  otlp:
    protocols: { grpc: {}, http: {} }
exporters:
  splunk_hec/traces:
    token: ${env:SPLUNK_HEC_TOKEN}
    endpoint: https://splunk.example.com:8088/services/collector
    source: fleetgraph
  signalfx:   # Splunk Observability Cloud metrics
    access_token: ${env:SPLUNK_ACCESS_TOKEN}
    realm: eu0
service:
  pipelines:
    traces:  { receivers: [otlp], exporters: [splunk_hec/traces] }
    metrics: { receivers: [otlp], exporters: [signalfx] }
```

Then set `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317` on the services. Keeping
the collector in between means the backend (Jaeger locally, Splunk in production) is a
deployment decision, not a code change.
