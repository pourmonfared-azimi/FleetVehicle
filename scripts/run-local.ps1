# Runs the three subgraphs and the gateway locally (no Docker, in-memory databases).
# Run ./scripts/compose.ps1 first so the gateway has a gateway.far.
$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')

$projects = @(
    'src/Vehicles/FleetGraph.Vehicles.Api',
    'src/Service/FleetGraph.Service.Api',
    'src/Parts/FleetGraph.Parts.Api'
)

foreach ($p in $projects) {
    Start-Process dotnet -ArgumentList "run --project $p" -WorkingDirectory $root
}

Start-Sleep -Seconds 8
Start-Process dotnet -ArgumentList 'run --project src/Gateway/FleetGraph.Gateway' -WorkingDirectory $root
Write-Host 'Gateway: http://localhost:5100/graphql'
