## Scope

Defines storage, data flow, and infrastructure constraints.

---

## Storage Model

### Entities

#### ShipmentHistory (table)

```
Id
Route
Carrier
Season
Packaging
Duration
TemperatureProfile
Excursion
Timestamp
```

#### ModelStore

```
ModelId
Version
SerializedModel
Metadata
```

#### AuditLog

```
AnalysisId
Timestamp
Input
Readiness
ModelVersion
Prediction
```

---

## Data Flow

```
Input → Persistence → HistoricalSelector → ML → Prediction → AuditLog
```

---

## Data Constraints

* Immutable historical records
* Append-only audit log
* Versioned models

---

## Infrastructure Requirements

### Compatibility

* .NET 10
* Native AOT

### Constraints

* No reflection-based serializers
* Use source generators

---

## Deployment Modes

* Local (SQLite / Lite DB)
* Cloud (Azure / OCI)

Same domain logic, different adapters.

---

## Observability

* Structured logging
* Correlation via AnalysisId
* Deterministic replay support

---