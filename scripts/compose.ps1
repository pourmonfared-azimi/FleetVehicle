# Exports the subgraph schemas and composes the Fusion supergraph (gateway.far).
#   ./scripts/compose.ps1                 -> Development (localhost:5101-5103)
#   ./scripts/compose.ps1 -Environment Docker
param([string]$Environment = 'Development')

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$archive = 'src/Gateway/FleetGraph.Gateway/gateway.far'
Push-Location $root
try {
    dotnet tool restore
    & (Join-Path $PSScriptRoot 'export-schemas.ps1')

    Write-Host "==> Composing supergraph ($Environment)"
    dotnet nitro fusion compose `
        --source-schema-file src/Vehicles/FleetGraph.Vehicles.Api/schema.graphqls `
        --source-schema-file src/Service/FleetGraph.Service.Api/schema.graphqls `
        --source-schema-file src/Parts/FleetGraph.Parts.Api/schema.graphqls `
        --archive $archive `
        --environment $Environment
    if ($LASTEXITCODE -ne 0) { throw 'Composition failed' }

    dotnet nitro fusion settings set default-list-size 25 --archive $archive --environment $Environment
    if ($LASTEXITCODE -ne 0) { throw 'Setting default-list-size failed' }

    Write-Host "==> Wrote $archive"
}
finally { Pop-Location }
