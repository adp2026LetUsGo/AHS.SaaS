# Xinfer Infra Audit — Diagnostic Gatekeeper (SSS-001/003)
# Compatible with PowerShell 5.1

$ErrorActionPreference = "Stop"
$apiPort = 53427

Write-Host "--- XINFER INFRA AUDIT START ---" -ForegroundColor Cyan

function Test-Endpoint {
    param($Name, $Body, $ExpectedStatus)
    Write-Host "TEST: $Name... " -NoNewline
    try {
        # UseBasicParsing is critical for PowerShell 5.1 compatibility in headless environments
        $res = Invoke-WebRequest -Uri "http://localhost:$apiPort/api/v1/predict" `
                                 -Method Post `
                                 -Body ($Body | ConvertTo-Json) `
                                 -ContentType "application/json" `
                                 -Headers @{ "X-Tenant-Id" = "audit" } `
                                 -UseBasicParsing
        $status = $res.StatusCode
        $content = $res.Content | ConvertFrom-Json
    } catch {
        $res = $_.Exception.Response
        if ($res) {
            $status = [int]$res.StatusCode
            # Use try-catch for stream reading as it varies by PS version
            try {
                $stream = $res.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($stream)
                $rawContent = $reader.ReadToEnd()
                try { $content = $rawContent | ConvertFrom-Json } catch { $content = $rawContent }
            } catch {
                $content = "Could not parse error body"
            }
        } else {
            Write-Host " CRITICAL: $($_.Exception.Message)" -ForegroundColor Red
            exit 1
        }
    }

    if ($status -eq $ExpectedStatus) {
        Write-Host "SUCCESS ($status)" -ForegroundColor Green
        return $content
    } else {
        Write-Host "FAIL (Expected $ExpectedStatus, got $status)" -ForegroundColor Red
        exit 1
    }
}

# SSS-001 Success
$body = @{
    route_id = "LHR-PTY-001"
    carrier = "TransPharma"
    external_temp_avg = 25.5
    transit_time_hrs = 12.0
    packaging_type = "passive"
    departure_timestamp = "2026-04-23T10:00:00Z"
}
$res = Test-Endpoint "SSS-001 Success" $body 200

# SSS-001 Readiness Fail (422)
$body.route_id = "SHORT-PTY-DAV"
Test-Endpoint "SSS-001 Readiness Fail (422)" $body 422

# SSS-001 Validation Fail (400)
$body.route_id = "LHR-PTY-001"
$body.external_temp_avg = 150
Test-Endpoint "SSS-001 Validation Fail (400)" $body 400

Write-Host "--- XINFER INFRA AUDIT SUCCESS ---" -ForegroundColor Cyan
exit 0
