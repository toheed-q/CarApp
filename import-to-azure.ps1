# ---------------------------------------------------------------------------
# Imports ACD.bacpac into your Azure SQL Database (dmf-sql-server / ACD).
# The password is requested at a secure prompt — it is NOT stored or printed.
#
# Run it from a normal terminal in this folder:
#   powershell -ExecutionPolicy Bypass -File .\import-to-azure.ps1
# ---------------------------------------------------------------------------

$ErrorActionPreference = 'Stop'

$sqlpackage = "$env:USERPROFILE\.dotnet\tools\sqlpackage.exe"
$bacpac     = "C:\Projects\CarApp\ACD.bacpac"
$server     = "tcp:dmf-sql-server.database.windows.net,1433"
$database   = "ACD"
$user       = "dmfadmin"

if (-not (Test-Path $bacpac)) { throw "BACPAC not found at $bacpac" }

# Securely prompt for the admin password (kept only in memory).
$secure = Read-Host "Enter the Azure SQL admin password for '$user'" -AsSecureString
$plain  = [System.Net.NetworkCredential]::new('', $secure).Password

$target = "Server=$server;Initial Catalog=$database;User ID=$user;Password=$plain;" +
          "Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;"

Write-Host ""
Write-Host "Importing $bacpac into Azure database '$database'..." -ForegroundColor Cyan
Write-Host "(First connection may take ~30-60s while the serverless DB wakes up.)" -ForegroundColor DarkGray
Write-Host ""

& $sqlpackage /Action:Import /SourceFile:"$bacpac" /TargetConnectionString:"$target"

Write-Host ""
Write-Host "Migration complete." -ForegroundColor Green
