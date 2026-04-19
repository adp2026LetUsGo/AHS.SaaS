## Traceability Scope

End-to-end trace: Input → Readiness → Model → Prediction → Output

---

## Correlation Model (MANDATORY)

```csharp
public sealed record AnalysisContext(
    Guid AnalysisId,
    DateTime Timestamp
);
```

---

## Audit Log (Canonical)

```csharp
public sealed record XinferAuditLog(
    Guid AnalysisId,
    DateTime Timestamp,
    ShipmentInput Input,
    ReadinessState Readiness,
    string ModelVersion,
    double? Prediction,
    IReadOnlyList<string> Explanation,
    IReadOnlyList<string> Recommendations
);
```

---

## Trace Mapping

| Stage          | Component            |
| -------------- | -------------------- |
| Input          | InputProcessor       |
| Readiness      | DataReadinessEngine  |
| Data Selection | HistoricalSelector   |
| Model          | ModelManager         |
| Prediction     | Predictor            |
| Explanation    | ExplanationEngine    |
| Recommendation | RecommendationEngine |
| Persistence    | PersistenceLayer     |

---

## Logging Requirements

* Input snapshot (full)
* Readiness decision
* Selected dataset metadata
* Model version used
* Prediction result
* Explanation output
* Recommendations

---

## Audit Requirements

* Reproducible inference
* Full historical traceability
* Deterministic re-execution
* Model version linkage
* AnalysisId correlation across all layers