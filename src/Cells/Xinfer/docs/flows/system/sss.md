# System Sequence Specification (SSS)
## Xinfer – Pre-Dispatch Thermal Risk Assessment

---

## 1. Scope

El sistema **Xinfer** permite predecir el riesgo de excursión térmica de un embarque farmacéutico antes del despacho.

Fuera de alcance:
- monitoreo en tiempo real
- telemetría en vuelo
- control físico del embarque

---

## 2. Actors

**Primary Actor:**
- Logistics User (Analyst / Operator)

**Secondary (implicit):**
- Control Tower (orchestrates, does NOT decide)

---

## 3. System Under Design

**System:** Xinfer Cell

**Type:**
- Autonomous Decision Cell
- Owns:
  - Data Readiness
  - Historical selection
  - Model lifecycle
  - Prediction
  - Explanation

---

## 4. Preconditions

- Shipment identity defined:
  - Product
  - Route
  - Date
  - Packaging

- Operational parameters provided:
  - Carrier
  - Departure time
  - Estimated duration

---

## 5. Main Success Scenario

1. User → System: SubmitShipmentContext(shipmentData)
2. User → System: SubmitOperationalParameters(operationalData)
3. User → System: RequestDataReadinessAnalysis()
4. System → System: EvaluateDatasetCompatibility()
5. System → User: ReturnDataReadinessStatus(status)

   status ∈ {Acceptable, Risky, NotAcceptable}

6. IF status = NotAcceptable:
   - System → User: RejectPrediction(reason)
   - END FLOW

7. System → System: SelectHistoricalDataset()
8. System → System: EvaluateRetrainingNeed()

9. IF retrainingRequired:
   - System → System: TrainModel()

10. System → System: PredictThermalRisk()
11. System → System: GenerateExplanation()
12. System → System: GenerateRecommendations()

13. System → User: ReturnRiskAssessment(
    probability,
    riskLevel,
    explanation,
    recommendations
)

---

## 6. Alternative Flows

### 6.1 Data Not Acceptable
- System blocks prediction
- User must adjust parameters

### 6.2 Risky Dataset
- Prediction allowed
- Confidence flagged as reduced

### 6.3 Insufficient Data
- System may degrade confidence
- Or fallback to baseline model

### 6.4 No Retraining Needed
- Existing model reused

---

## 7. Internal Responsibilities

- Dataset compatibility analysis
- Detection of inconsistent data
- Automatic historical filtering
- Model version control
- Training decision logic

---

## 8. Business Rules

**RULE-01:** No prediction if DataReadinessStatus = NotAcceptable

**RULE-02:** Dataset validation before inference

**RULE-03:** Historical selection is automatic

**RULE-04:** Retraining only when justified

**RULE-05:** System may refuse prediction

**RULE-06:**
- Shipment Identity:
  - Product, Route, Date, Packaging
- Risk Modifiers:
  - Carrier, Time, Duration, Environment

---

## 9. Postconditions

- Risk probability calculated
- Risk level assigned
- Explanation generated
- Recommendations provided
- Model version traceable

---

## 10. System Contracts

### Shipment Context
```json
{
  "product": "string",
  "route": {
    "origin": "string",
    "destination": "string"
  },
  "date": "datetime",
  "packaging": "string"
}
```

### Operational Parameters
```json
{
  "carrier": "string",
  "departure_time": "datetime",
  "estimated_duration_hours": "number"
}
```

### Data Readiness Response
```json
{
  "status": "Acceptable | Risky | NotAcceptable",
  "issues": ["string"],
  "recommendations": ["string"]
}
```

### Risk Assessment Response
```json
{
  "probability": 0.0,
  "risk_level": "Low | Medium | High",
  "explanation": "string",
  "recommendations": ["string"],
  "model_version": "string"
}
```

---

## 11. Architecture Notes

- Xinfer is a self-contained Cell
- Own database and models
- No shared models across domains
- Control Tower orchestrates only

---

## 12. Traceability

System logs:
- Inputs
- Data Readiness result
- Dataset used
- Model version
- Prediction output
