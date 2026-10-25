# Exports each subgraph's source schema (schema.graphqls) into its project folder.
$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
Push-Location $root
try {
    foreach ($s in 'Vehicles', 'Service', 'Parts') {
        Write-Host "==> Exporting $s"
        dotnet run --project "src/$s/FleetGraph.$s.Api" --no-launch-profile -- `
            schema export --output "src/$s/FleetGraph.$s.Api/schema.graphqls"
        if ($LASTEXITCODE -ne 0) { throw "Schema export failed for $s" }
    }
}
finally { Pop-Location }
