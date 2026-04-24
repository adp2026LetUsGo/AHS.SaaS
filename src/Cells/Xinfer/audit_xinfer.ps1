# Master Diagnostic Auditor — Xinfer Cell
# This script validates the integrity of the Xinfer Cell (Backend + Frontend + Contracts)
# Standard: ADR-009 Closed Cycle Architecture

$ErrorActionPreference = "Stop"

Write-Host "--- XINFER MASTER AUDIT START ---" -ForegroundColor Cyan

# 1. Connectivity Check
Write-Host "[1/3] Checking Connectivity..." -ForegroundColor Yellow
$apiPort = 53427
$uiPort = 5173

$apiOk = Test-NetConnection -ComputerName localhost -Port $apiPort -InformationLevel Quiet
if (-not $apiOk) {
    Write-Error "CRITICAL: Xinfer API not listening on port $apiPort"
}
Write-Host "✅ API Port $apiPort reachable." -ForegroundColor Green

$uiOk = Test-NetConnection -ComputerName localhost -Port $uiPort -InformationLevel Quiet
if (-not $uiOk) {
    Write-Warning "WARNING: UI Demo not listening on port $uiPort. (Optional for API audit)"
} else {
    Write-Host "✅ UI Port $uiPort reachable." -ForegroundColor Green
}

# 2. Health Endpoint Check
Write-Host "[2/3] Checking Operational Health..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:$apiPort/health" -Method Get
    if (-not $health.healthy) {
        Write-Error "CRITICAL: API Health endpoint returned healthy: $($health.healthy)"
    }
    Write-Host "✅ API Health: Healthy" -ForegroundColor Green
} catch {
    Write-Error "CRITICAL: Failed to reach /health endpoint. $($_.Exception.Message)"
}

# 3. Contract Integrity Check (inference_v1)
Write-Host "[3/3] Validating Contract (inference_v1)..." -ForegroundColor Yellow
$testBody = @{
    route_id = "AUDIT-TEST-001"
    carrier = "AHS-Audit"
    external_temp_avg = 32.5
    transit_time_hrs = 24.0
    packaging_type = "passive"
    departure_timestamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
} | ConvertTo-Json

try {
    $json = Invoke-RestMethod -Uri "http://localhost:$apiPort/api/v1/predict" `
                                  -Method Post `
                                  -Body $testBody `
                                  -ContentType "application/json" `
                                  -Headers @{ "X-Tenant-Id" = "audit-tenant" }

    # Validate Envelope Structure
    if (-not $json.metadata) { Write-Error "MISSING: metadata object" }
    if (-not $json.data) { Write-Error "MISSING: data object" }
    if (-not $json.status) { Write-Error "MISSING: status object" }

    # Validate Business Fields (inference_v1 - snake_case)
    Write-Host "   - Checking data.risk_score..." -NoNewline
    if ($null -eq $json.data.risk_score) { Write-Error "FAIL" } else { Write-Host " OK" -ForegroundColor Green }

    Write-Host "   - Checking data.confidence_score..." -NoNewline
    if ($null -eq $json.data.confidence_score) { Write-Error "FAIL" } else { Write-Host " OK" -ForegroundColor Green }

    Write-Host "   - Checking data.influence_factors..." -NoNewline
    if ($json.data.influence_factors.Count -eq 0) { Write-Error "FAIL (Empty)" } else { Write-Host " OK" -ForegroundColor Green }

    Write-Host "   - Checking metadata.contract_version..." -NoNewline
    if ($json.metadata.contract_version -ne "inference_v1") { 
        Write-Error "FAIL (Expected 'inference_v1', got '$($json.metadata.contract_version)')" 
    } else { Write-Host " OK" -ForegroundColor Green }

    Write-Host "✅ Contract inference_v1 validated successfully." -ForegroundColor Green

} catch {
    Write-Error "CRITICAL: API contract validation failed. $($_.Exception.Message)"
}

Write-Host "--- XINFER MASTER AUDIT SUCCESS ---" -ForegroundColor Cyan
exit 0
