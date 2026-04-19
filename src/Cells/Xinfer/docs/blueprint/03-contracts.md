# Contracts

- API Endpoint: POST /analyze
- Request: `Shipment` DTO
- Response: `ReadinessResult` DTO
- Tipos fuertemente tipados, sin reflection ni dynamic (AOT-friendly)


## Input

```
ShipmentInput
```

## Output

```
XinferResult {
  readinessStatus
  riskProbability
  riskLevel
  explanation
  recommendations
}
```