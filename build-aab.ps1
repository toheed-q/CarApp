# ---------------------------------------------------------------------------
# Builds a SIGNED Android App Bundle (.aab) for Play Store upload.
# Asks for the keystore password securely (never stored or printed).
#
# Run:  powershell -ExecutionPolicy Bypass -File .\build-aab.ps1
# ---------------------------------------------------------------------------

$ErrorActionPreference = 'Stop'

$proj     = "C:\Projects\CarApp\Maui App\DMF\DMF.csproj"
$keystore = "C:\Projects\CarApp\dmf-release.keystore"
$alias    = "dmfkey"
$outDir   = "C:\Projects\CarApp\Maui App\DMF\bin\Release\net9.0-android"
$desktop  = "C:\Users\HC\Desktop\dmf-services.aab"

if (-not (Test-Path $keystore)) { throw "Keystore not found at $keystore" }

$secure = Read-Host "Enter the keystore password" -AsSecureString
$ksPwd  = [System.Net.NetworkCredential]::new('', $secure).Password

Write-Host ""
Write-Host "Building signed AAB... (5 to 10 min, please wait)" -ForegroundColor Cyan
Write-Host ""

$dotnetArgs = @(
    'publish', $proj,
    '-f', 'net9.0-android',
    '-c', 'Release',
    '-p:AndroidPackageFormat=aab',
    '-p:AndroidKeyStore=true',
    "-p:AndroidSigningKeyStore=$keystore",
    "-p:AndroidSigningKeyAlias=$alias",
    "-p:AndroidSigningStorePass=$ksPwd",
    "-p:AndroidSigningKeyPass=$ksPwd"
)

& dotnet @dotnetArgs

# Locate the produced .aab (prefer the signed one).
$aab = Get-ChildItem $outDir -Recurse -Filter *.aab -ErrorAction SilentlyContinue |
       Sort-Object { $_.Name -like '*Signed*' } -Descending |
       Select-Object -First 1

if ($aab) {
    Copy-Item $aab.FullName $desktop -Force
    Write-Host ""
    Write-Host "SUCCESS. Signed AAB copied to Desktop:" -ForegroundColor Green
    Write-Host "  $desktop" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Build finished but no .aab found. Paste the output so we can check." -ForegroundColor Yellow
}
