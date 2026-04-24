# Data Contract: Xinfer Prediction Output (Inference)
**Contract Version: inference_v1**

This contract defines the schema for the output of a thermal excursion risk prediction, wrapped in the AHS Global Universal Envelope.

## 1. Structure (Universal Envelope)

### Metadata
- `cell_id`: `xinfer`
- `contract_version`: `inference_v1`
- `timestamp`: Response generation time (ISO 8601 UTC).
- `request_id`: Parent Request/Correlation ID.

### Status
- Standardized `code`, `error_code`, `message`, and `trace_id`.

## 2. Business Data Payload (`data`)

The `data` object contains the core statistical outcome of the inference engine.

| Property | Type | Mandatory | Range / Validation | Description |
| :--- | :--- | :---: | :--- | :--- |
| `risk_score` | Float | Yes | 0.0 to 1.0 | Normalized probability of thermal excursion. |
| `risk_level` | Enum | Yes | `low`, `medium`, `high` | Qualitative categorization of risk based on the score. |
| `confidence_score` | Float | Yes | 0.0 to 1.0 | Model certainty about its own prediction. Values < 0.6 must trigger a UI caution state. |
| `influence_factors` | Array | Yes | List of Factor Objects | Breakdown of variables contributing to the final score. |
| `model_metadata` | Object | Yes | Traceability fields | Information about the ML model used for inference. |

### Influence Factor Object
| Property | Type | Description |
| :--- | :--- | :--- |
| `factor` | String | Name of the feature (e.g., `EXTERNAL_TEMP`, `AIR_CARRIER_RELIABILITY`). |
| `weight` | Float | Magnitude of influence (-1.0 to 1.0). Positive increases risk, negative decreases it. |

### Model Metadata Object
| Property | Type | Description |
| :--- | :--- | :--- |
| `model_version` | String | Semantic version or hash of the model deployment. |
| `trained_at` | String | ISO 8601 UTC timestamp of the model's last training completion. |
| `accuracy_metric` | Float | The accuracy/confidence metric of the model at the time of inference. |

## 3. JSON Example
```json
{
  "metadata": {
    "cell_id": "xinfer",
    "contract_version": "inference_v1",
    "timestamp": "2026-04-21T05:40:00Z",
    "request_id": "01HHT-XINFER-998877"
  },
  "data": {
    "risk_score": 0.125,
    "risk_level": "low",
    "confidence_score": 0.91,
    "influence_factors": [
      { "factor": "EXTERNAL_TEMP", "weight": 0.45 },
      { "factor": "CARRIER_PERFORMANCE", "weight": -0.22 },
      { "factor": "PACKAGING_INSULATION", "weight": -0.85 }
    ],
    "model_metadata": {
      "model_version": "v3.1.2-alpha",
      "trained_at": "2026-04-18T10:00:00Z",
      "accuracy_metric": 0.942
    }
  },
  "status": {
    "code": 200,
    "error_code": null,
    "message": "Inference generated successfully",
    "trace_id": "01HHT-XINFER-998877"
  }
}
```

---
*Authorized by: Senior Data Engineer*
