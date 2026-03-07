# ============================================================================
# AHS.SaaS ECOSYSTEM - GOLD MASTER IGNITION SCRIPT (V3.1 - CLEAN VERSION)
# ============================================================================

$ErrorActionPreference = "Stop"
$ProjectRoot = Get-Location

# 1. ELEVACION DE PRIVILEGIOS (FIREWALL)
Write-Host "[FIREWALL] Configurando reglas de red..." -ForegroundColor Cyan
try {
    Remove-NetFirewallRule -DisplayName "AHS_SaaS_API" -ErrorAction SilentlyContinue
    Remove-NetFirewallRule -DisplayName "AHS_SaaS_UI" -ErrorAction SilentlyContinue

    New-NetFirewallRule -DisplayName "AHS_SaaS_API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
    New-NetFirewallRule -DisplayName "AHS_SaaS_UI" -Direction Inbound -LocalPort 5120 -Protocol TCP -Action Allow
    Write-Host "OK: Puertos 5000 y 5120 habilitados." -ForegroundColor Green
} catch {
    Write-Host "ERROR: Ejecute como Administrador para configurar el Firewall." -ForegroundColor Red
}

# 2. COMPILACION NATIVA (GOLD MASTER BUILD)
Write-Host "[BUILD] Compilando Backend (Native AOT - SIMD Optimized)..." -ForegroundColor Cyan
dotnet publish src/Presentation/AHS.Gateway.API/AHS.Gateway.API.csproj -c Release -r win-x64 --self-contained true /p:PublishAot=true

Write-Host "[BUILD] Compilando Frontend (BentoUI - Release Mode)..." -ForegroundColor Cyan
dotnet build src/Presentation/AHS.Web.BentoUI/AHS.Web.BentoUI.csproj -c Release

# 3. LANZAMIENTO DEL BACKEND (HPC ENGINE)
Write-Host "[SERVER] Lanzando Backend API en puerto 5000..." -ForegroundColor Yellow
$ApiPublishDir = "$ProjectRoot\src\Presentation\AHS.Gateway.API\bin\Release\net10.0\win-x64\publish"
Start-Process -FilePath "$ApiPublishDir\AHS.Gateway.API.exe" -ArgumentList "--urls http://0.0.0.0:5000" -WindowStyle Normal

Start-Sleep -Seconds 3

# 4. LANZAMIENTO DEL FRONTEND (DASHBOARD ELITE)
Write-Host "[SERVER] Lanzando Universal Control Tower en puerto 5120..." -ForegroundColor Yellow
Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Presentation/AHS.Web.BentoUI/AHS.Web.BentoUI.csproj -c Release --urls http://0.0.0.0:5120" -WindowStyle Normal

# 5. ACTIVACION
Write-Host "[READY] Abriendo Centro de Comando..." -ForegroundColor Green
Start-Sleep -Seconds 5
Start-Process "http://localhost:5120/command-center"

Write-Host "AHS.SaaS GOLD MASTER OPERATIVO." -ForegroundColor White -BackgroundColor Blue