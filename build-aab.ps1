# ---------------------------------------------------------------------------
# Builds a VERIFIED-SIGNED Android App Bundle (.aab) for Play Store - one shot.
# Cleans, builds (auto-retries the flaky net10 resource glitch), makes sure the
# .aab is signed, verifies the signature, and drops it on the Desktop.
# Asks for the keystore password once (never stored or printed).
#
# Run:  powershell -ExecutionPolicy Bypass -File .\build-aab.ps1
# ---------------------------------------------------------------------------

$ErrorActionPreference = 'Stop'

$proj      = "E:\Projects\CarApp\Maui App\DMF\DMF.csproj"
$keystore  = "E:\Projects\CarApp\dmf-release.keystore"
$alias     = "dmfkey"
$jarsigner = "C:\Program Files\Microsoft\jdk-21.0.11.10-hotspot\bin\jarsigner.exe"
$objDir    = "E:\Projects\CarApp\Maui App\DMF\obj"
$binDir    = "E:\Projects\CarApp\Maui App\DMF\bin"
$outDir    = "E:\Projects\CarApp\Maui App\DMF\bin\Release\net10.0-android"
$desktop   = "C:\Users\HC\Desktop\dmf-services.aab"

if (-not (Test-Path $keystore))  { throw "Keystore not found at $keystore" }
if (-not (Test-Path $jarsigner)) { throw "jarsigner not found at $jarsigner" }

$secure = Read-Host "Enter the keystore password" -AsSecureString
$ksPwd  = [System.Net.NetworkCredential]::new('', $secure).Password

Write-Host ""
Write-Host "Cleaning old build output..." -ForegroundColor Cyan
try { dotnet build-server shutdown | Out-Null } catch {}
foreach ($p in @($objDir, $binDir)) {
    if (Test-Path $p) { Remove-Item $p -Recurse -Force -ErrorAction SilentlyContinue }
}

$dotnetArgs = @(
    'publish', $proj,
    '-f', 'net10.0-android',
    '-c', 'Release',
    '-p:AndroidPackageFormat=aab',
    '-p:AndroidKeyStore=true',
    "-p:AndroidSigningKeyStore=$keystore",
    "-p:AndroidSigningKeyAlias=$alias",
    "-p:AndroidSigningStorePass=$ksPwd",
    "-p:AndroidSigningKeyPass=$ksPwd"
)

# net10 sometimes fails the first resource-link pass; a second run passes.
$built = $false
for ($try = 1; $try -le 3 -and -not $built; $try++) {
    Write-Host ""
    Write-Host "Building signed AAB (attempt $try of 3)... 5 to 10 min, please wait." -ForegroundColor Cyan
    Write-Host ""
    & dotnet @dotnetArgs
    if ($LASTEXITCODE -eq 0) { $built = $true } else { Write-Host "Build hit the transient net10 glitch - retrying..." -ForegroundColor Yellow }
}
if (-not $built) { throw "Build failed 3 times. Copy the red error lines and send them over." }

# Find the produced .aab (prefer a *-Signed one).
$aab = Get-ChildItem $outDir -Recurse -Filter *.aab -ErrorAction SilentlyContinue |
       Sort-Object { $_.Name -like '*Signed*' } -Descending |
       Select-Object -First 1
if (-not $aab) { throw "Build finished but no .aab was produced." }

if (Test-Path $desktop) { Remove-Item $desktop -Force }

# Is it already signed by the build? If not, sign it ourselves.
$verify = & $jarsigner -verify $aab.FullName 2>&1
if ($verify -match 'jar verified') {
    Write-Host "AAB is signed by the build." -ForegroundColor Green
    Copy-Item $aab.FullName $desktop -Force
} else {
    Write-Host "AAB not signed by the build - signing with the release key..." -ForegroundColor Yellow
    & $jarsigner -keystore $keystore -storepass $ksPwd -keypass $ksPwd `
        -signedjar $desktop $aab.FullName $alias
    if ($LASTEXITCODE -ne 0) { throw "Signing failed (wrong password or alias?)." }
}

# Final proof: the Desktop file must verify.
$final = & $jarsigner -verify $desktop 2>&1
if ($final -match 'jar verified') {
    Write-Host ""
    Write-Host "SUCCESS - verified SIGNED AAB on your Desktop:" -ForegroundColor Green
    Write-Host "  $desktop" -ForegroundColor Green
    Write-Host "Play Store will accept this file." -ForegroundColor Green
} else {
    throw "The final AAB did not pass signature verification. Do not upload it; send the output."
}
