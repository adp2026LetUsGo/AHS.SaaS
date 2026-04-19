## Scope

Defines how models are built, trained, evaluated, versioned, and used within Xinfer.

---

## Label Definition

```
excursion = true | false
```

Binary classification: thermal excursion occurred or not.

---

## Feature Engineering (Deterministic)

### Core Features

```
route (encoded)
carrier (encoded)
season (derived from departureDate)
packaging (categorical)
duration (numeric)
```

### Derived Features

```
route_risk_index
carrier_performance_score
seasonal_risk_factor
```

### Rules

* No dynamic feature generation at runtime
* All features must be reproducible
* Feature set must be versioned

---

## Dataset Construction

### Input

* Output from HistoricalSelector

### Filtering Rules

* Must pass Data Readiness
* Homogeneous route
* Consistent season

### Minimum Size

* < 5 → reject
* 5–10 → low confidence
* > 10 → valid

---

## Training Strategy

### Approach

* On-demand training (triggered by inference if needed)
* Rolling window dataset

### Conditions to Train

* No existing model
* Model outdated
* Dataset significantly changed

---

## Model Types

### Baseline

* Logistic Regression

### Advanced

* Gradient Boosting

### Constraints

* Deterministic behavior
* AOT-compatible (no dynamic loading)

---

## Evaluation Metrics

### Required

```
Accuracy
Recall (CRITICAL)
Precision
```

### Priority

* Minimize false negatives

---

## Model Versioning

```
modelId
version
dataWindow
trainedAt
metrics
```

Rules:

* Every model is immutable
* Version tied to dataset snapshot

---

## Prediction Rules

* Only execute if Readiness ≠ NotAcceptable
* Always log model version

---

## Retraining Policy

* Time-based (configurable)
* Drift-based (future)
* Data-volume-based

---