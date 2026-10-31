# Reset the demo: stop the demo app, drop the throwaway demo database, start the app again on an EMPTY database.
# The app then runs the DatabaseInitializer (Migrate + Seed) exactly as a first-time user would see it.
# Local only. Uses LocalDB database "FastEndpointsDemo" (never the owner's FastEndpointsAPI database).
param([string]$Work = "$PSScriptRoot\..\work")

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path "$PSScriptRoot\..\..\..\..\..").Path
$db = 'FastEndpointsDemo'
$server = '(localdb)\mssqllocaldb'

# 1. stop any running demo instance
Get-CimInstance Win32_Process |
  Where-Object { $_.CommandLine -like '*VerticalSliceFastEndpoints.dll*' -or $_.CommandLine -like '*run --project VerticalSliceFastEndpoints*' } |
  ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
Start-Sleep -Seconds 2

# 2. drop the demo database (only this one)
sqlcmd -S $server -Q "IF DB_ID('$db') IS NOT NULL BEGIN ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$db]; END" | Out-Null

# 3. start the app with the demo connection string
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://localhost:5156'
$env:ConnectionStrings__DefaultConnection = "Server=$server;Database=$db;Trusted_Connection=true;TrustServerCertificate=true;"
New-Item -ItemType Directory -Force -Path $Work | Out-Null
Remove-Item "$Work\app-run.log", "$Work\app-run.err" -ErrorAction SilentlyContinue
Start-Process -FilePath dotnet -ArgumentList 'run --project VerticalSliceFastEndpoints --no-launch-profile' `
  -WorkingDirectory $repo -RedirectStandardOutput "$Work\app-run.log" -RedirectStandardError "$Work\app-run.err" -WindowStyle Hidden

# 4. wait until it answers
$ok = $false
for ($i = 0; $i -lt 90; $i++) {
  Start-Sleep -Seconds 1
  try { $r = Invoke-WebRequest 'http://localhost:5156/api/v1/products' -UseBasicParsing -TimeoutSec 2; if ($r.StatusCode -eq 200) { $ok = $true; break } } catch { }
}
if (-not $ok) { Write-Output 'RESET FAIL: app did not start'; exit 1 }
$body = $r.Content
Write-Output "RESET PASS: app is up on an empty database. GET /api/v1/products -> $body"
