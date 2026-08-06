# ---------------------------------------------------------------------------
# Signs the already-built (but unsigned) .aab with the release upload key.
# Asks for the keystore password securely (never stored or printed).
#
# Run:  powershell -ExecutionPolicy Bypass -File .\sign-aab.ps1
# ---------------------------------------------------------------------------

$ErrorActionPreference = 'Stop'

$jarsigner = "C:\Program Files\Microsoft\jdk-21.0.11.10-hotspot\bin\jarsigner.exe"
$keystore  = "C:\Projects\CarApp\dmf-release.keystore"
$alias     = "dmfkey"
$inAab     = "C:\Projects\CarApp\Maui App\DMF\bin\Release\net10.0-android\com.dmf.services.aab"
$outAab    = "C:\Users\HC\Desktop\dmf-services-signed.aab"

if (-not (Test-Path $jarsigner)) { throw "jarsigner not found at $jarsigner" }
if (-not (Test-Path $keystore))  { throw "Keystore not found at $keystore" }
if (-not (Test-Path $inAab))     { throw "Unsigned AAB not found at $inAab" }

$secure = Read-Host "Enter the keystore password" -AsSecureString
$ksPwd  = [System.Net.NetworkCredential]::new('', $secure).Password

if (Test-Path $outAab) { Remove-Item $outAab -Force }

Write-Host ""
Write-Host "Signing AAB..." -ForegroundColor Cyan

& $jarsigner -keystore $keystore -storepass $ksPwd -keypass $ksPwd `
    -signedjar $outAab $inAab $alias

if ($LASTEXITCODE -ne 0) { throw "jarsigner failed (wrong password or alias?). Exit code $LASTEXITCODE" }

Write-Host ""
Write-Host "Verifying signature..." -ForegroundColor Cyan
& $jarsigner -verify $outAab

if ($LASTEXITCODE -ne 0) { throw "Verification failed." }

Write-Host ""
Write-Host "SUCCESS. Signed AAB is on your Desktop:" -ForegroundColor Green
Write-Host "  $outAab" -ForegroundColor Green
Write-Host "Upload THIS file to Play Console." -ForegroundColor Green
