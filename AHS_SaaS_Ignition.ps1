# ============================================================================
# AHS.SaaS ECOSYSTEM - MASTER IGNITION SCRIPT (V4.0 - NEUTRAL ID)
# Purpose: Build and Launch the Sovereign Control Tower (AHS.Web.UI)
# ============================================================================

$ErrorActionPreference = "Stop"
$ProjectRoot = Get-Location

# 1. FIREWALL CONFIGURATION
Write-Host "[NETWORK] Configuring Firewall rules (Ports 5000, 5120)..." -ForegroundColor Cyan
try {
    Remove-NetFirewallRule -DisplayName "AHS_SaaS_API" -ErrorAction SilentlyContinue
    Remove-NetFirewallRule -DisplayName "AHS_SaaS_UI" -ErrorAction SilentlyContinue
    New-NetFirewallRule -DisplayName "AHS_SaaS_API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
    New-NetFirewallRule -DisplayName "AHS_SaaS_UI" -Direction Inbound -LocalPort 5120 -Protocol TCP -Action Allow
    Write-Host "OK: Network lanes secured." -ForegroundColor Green
}
catch {
    Write-Host "NOTE: Run as Administrator for Firewall rules. Skipping..." -ForegroundColor Yellow
}

# 2. MASTER BUILD (RELEASE MODE)
Write-Host "[BUILD] Compiling Backend (Native AOT - SIMD)..." -ForegroundColor Cyan
dotnet publish src/Presentation/AHS.Gateway.API/AHS.Gateway.API.csproj -c Release -r win-x64 --self-contained true /p:PublishAot=true

Write-Host "[BUILD] Compiling Frontend (AHS.Web.UI - Release)..." -ForegroundColor Cyan
dotnet build src/Presentation/AHS.Web.UI/AHS.Web.UI.csproj -c Release

# 3. BACKEND LAUNCH (HPC ENGINE)
Write-Host "[LAUNCH] Starting API Gateway on Port 5000..." -ForegroundColor Yellow
$ApiPublishDir = "$ProjectRoot\src\Presentation\AHS.Gateway.API\bin\Release\net10.0\win-x64\publish"
Start-Process -FilePath "$ApiPublishDir\AHS.Gateway.API.exe" -ArgumentList "--urls http://0.0.0.0:5000" -WindowStyle Normal

Start-Sleep -Seconds 3

# 4. FRONTEND LAUNCH (SOVEREIGN CONSOLE)
Write-Host "[LAUNCH] Starting Control Tower on Port 5120..." -ForegroundColor Yellow
Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Presentation/AHS.Web.UI/AHS.Web.UI.csproj -c Release --urls http://0.0.0.0:5120" -WindowStyle Normal

# 5. BROWSER ACTIVATION
Write-Host "[READY] Opening Universal Control Tower..." -ForegroundColor Green
Start-Sleep -Seconds 5
Start-Process "http://localhost:5120/"

Write-Host "AHS.SaaS SOVEREIGN CONSOLE ACTIVE." -ForegroundColor White -BackgroundColor Blue