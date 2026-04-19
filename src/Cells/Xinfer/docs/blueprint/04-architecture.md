## Architecture Scope

Backend / Core system only (NO UI concerns)

---

## Xinfer Cell

```
XinferCell
 ├── IDataReadinessEngine
 ├── IHistoricalSelector
 ├── IModelManager
 │    ├── ITrainer
 │    └── IPredictor
 ├── IExplanationEngine
 ├── IRecommendationEngine
 └── IPersistenceLayer
```

---

## Core Interfaces (AOT-safe)

```csharp
public interface IDataReadinessEngine
{
    ReadinessState Evaluate(ShipmentInput input);
}

public interface IHistoricalSelector
{
    IReadOnlyList<ShipmentHistory> Select(ShipmentInput input);
}

public interface IModelManager
{
    ModelMetadata GetOrTrain(IReadOnlyList<ShipmentHistory> data);
}

public interface IPredictor
{
    double Predict(ModelMetadata model, ShipmentInput input);
}

public interface IExplanationEngine
{
    IReadOnlyList<string> Explain(double prediction);
}

public interface IRecommendationEngine
{
    IReadOnlyList<string> Recommend(ReadinessState state, double? prediction);
}
```

---

## Readiness State Model (Canonical)

```csharp
public abstract record ReadinessState;

public sealed record NotAcceptableState(
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> Recommendations
) : ReadinessState;

public sealed record RiskyState(
    string Explanation
) : ReadinessState;

public sealed record AcceptableState() : ReadinessState;
```

---

## Data Readiness Rules (Deterministic)

### Sample Size

* < 5 → NotAcceptable
* 5–10 → Risky
* > 10 → Acceptable

### Season Consistency

* Mixed seasons (multiple seasonal clusters) → NotAcceptable

### Carrier Variance

* Variance > threshold → Risky
* Variance >> threshold → NotAcceptable

### Route Consistency

* Different climate zones → NotAcceptable

### Temporal Relevance

* Data older than threshold → Risky

---

## System Flow (Execution)

```
1. Receive ShipmentInput
2. Evaluate DataReadiness
3. IF NotAcceptable → return XinferResult (no prediction)
4. Select Historical Data
5. Get or Train Model
6. Predict Risk
7. Generate Explanation
8. Generate Recommendations
9. Return XinferResult