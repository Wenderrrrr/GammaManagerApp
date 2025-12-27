# PowerShell script to prepare files for MSIX packaging
# Run this script to create the proper folder structure for MSIX

Write-Host "Preparing Gamma Manager for MSIX Packaging..." -ForegroundColor Green

# Create MSIX folder structure
$msixDir = "MSIX_Package"
$assetsDir = "$msixDir\Assets"

if (Test-Path $msixDir) {
    Remove-Item $msixDir -Recurse -Force
}

New-Item -ItemType Directory -Path $msixDir -Force | Out-Null
New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null

Write-Host "Copying application files..." -ForegroundColor Yellow

# Copy application files from Release build
$sourceDir = "Gamma Manager\bin\Release"
if (Test-Path $sourceDir) {
    Copy-Item "$sourceDir\Gamma Manager.exe" -Destination $msixDir
    Copy-Item "$sourceDir\*.dll" -Destination $msixDir
    Copy-Item "$sourceDir\frontend" -Destination $msixDir -Recurse -Force
    Copy-Item "$sourceDir\*.config" -Destination $msixDir -ErrorAction SilentlyContinue
    Write-Host "✓ Application files copied" -ForegroundColor Green
} else {
    Write-Host "✗ Release build not found. Please build the project first!" -ForegroundColor Red
    exit 1
}

# Copy manifest
if (Test-Path "Package.appxmanifest") {
    Copy-Item "Package.appxmanifest" -Destination $msixDir
    Write-Host "✓ Manifest copied" -ForegroundColor Green
}

# Create placeholder images if icon exists
if (Test-Path "Gamma Manager\TrayIcon.ico") {
    Write-Host "Note: Please create PNG versions of your icon:" -ForegroundColor Yellow
    Write-Host "  - Square44x44Logo.png (44x44)"
    Write-Host "  - Square150x150Logo.png (150x150)"
    Write-Host "  - Wide310x150Logo.png (310x150)"
    Write-Host "  - SplashScreen.png (620x300)"
    Write-Host "Place them in: $assetsDir" -ForegroundColor Yellow
}

Write-Host "`n✓ MSIX package folder structure created!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Create required PNG images and place in $assetsDir"
Write-Host "2. Edit Package.appxmanifest to update Publisher and other details"
Write-Host "3. Use MSIX Packaging Tool or run makeappx to create the package"
Write-Host "`nTo create MSIX package with makeappx:"
Write-Host '  makeappx pack /d "MSIX_Package" /p "GammaManager.msix"' -ForegroundColor White
Write-Host "`nFor testing, you can self-sign with:"
Write-Host '  signtool sign /fd SHA256 /a /f TestCert.pfx "GammaManager.msix"' -ForegroundColor White
