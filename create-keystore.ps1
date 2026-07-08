# ---------------------------------------------------------------------------
# Creates the release signing keystore (upload key) for the DMF app.
# The password is asked at a secure prompt and is NEVER stored or printed.
#
# Run it from a terminal in this folder:
#   powershell -ExecutionPolicy Bypass -File .\create-keystore.ps1
# ---------------------------------------------------------------------------

$ErrorActionPreference = 'Stop'

$keytool  = "C:\Program Files\Microsoft\jdk-21.0.11.10-hotspot\bin\keytool.exe"
$keystore = "C:\Projects\CarApp\dmf-release.keystore"
$alias    = "dmfkey"

if (-not (Test-Path $keytool)) { throw "keytool not found at $keytool" }

if (Test-Path $keystore) {
    Write-Host "A keystore already exists at:" -ForegroundColor Yellow
    Write-Host "  $keystore" -ForegroundColor Yellow
    Write-Host "Do NOT recreate it if you have already uploaded a build with it." -ForegroundColor Yellow
    return
}

$p1 = Read-Host "Choose a keystore password (remember it!)" -AsSecureString
$p2 = Read-Host "Re-enter the password" -AsSecureString
$s1 = [System.Net.NetworkCredential]::new('', $p1).Password
$s2 = [System.Net.NetworkCredential]::new('', $p2).Password

if ($s1 -ne $s2)        { throw "Passwords do not match." }
if ($s1.Length -lt 6)   { throw "Password must be at least 6 characters." }

& $keytool -genkeypair -v `
    -keystore $keystore `
    -alias $alias `
    -keyalg RSA -keysize 2048 -validity 10000 `
    -storepass $s1 -keypass $s1 `
    -dname "CN=DMF Services, OU=Mobile, O=DMF Services, L=NA, ST=NA, C=IN"

Write-Host ""
Write-Host "Keystore created:  $keystore" -ForegroundColor Green
Write-Host "Alias:             $alias" -ForegroundColor Green
Write-Host ""
Write-Host "!! BACK UP this file AND remember the password. !!" -ForegroundColor Red
Write-Host "   If you lose either, you cannot publish updates to your app later." -ForegroundColor Red
