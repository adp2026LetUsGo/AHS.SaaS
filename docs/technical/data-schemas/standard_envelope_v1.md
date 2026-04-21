# Global Data Contract Standard: Universal Envelope
**Version: v1**

All cell responses within the AHS SaaS ecosystem must follow this "Envelope" pattern to ensure uniform observability, traceability, and error handling.

## 1. Structure Definition
The response is divided into three primary top-level objects: `metadata`, `data`, and `status`.

### Metadata Section
Contains environment and routing information.
- `cell_id`: (string) Unique identifier of the source cell (e.g., `xinfer`).
- `contract_version`: (string) The version of the contract used to generate the `data` payload (e.g., `shipment_v1`).
- `timestamp`: (string) ISO 8601 UTC timestamp of the response generation.
- `request_id`: (string) GUID/ULID for end-to-end traceability across the ecosystem.

### Data Section
- `data`: (object|array) The specific business payload. The structure of this object is defined by the `contract_version`.

### Status Section
Standardized outcome of the operation.
- `code`: (integer) HTTP-compatible status code (e.g., `200`, `400`, `500`).
- `error_code`: (string|null) Business-logic specific error code (e.g., `ERR_INSUFFICIENT_DATA`, `ERR_MODEL_STALE`).
- `message`: (string) Human-readable explanation of the status.
- `trace_id`: (string) Correlation ID for log aggregation (often maps to `request_id`).

## 2. JSON Example
```json
{
  "metadata": {
    "cell_id": "xinfer",
    "contract_version": "standard_envelope_v1",
    "timestamp": "2026-04-21T03:00:00.000Z",
    "request_id": "01HHT-ABC123-EFG456"
  },
  "data": {
    "shipment_id": "b0e4a7d1-...",
    "prediction": "excursion_risk",
    "confidence_score": 0.942
  },
  "status": {
    "code": 200,
    "error_code": null,
    "message": "Inference generated successfully",
    "trace_id": "01HHT-ABC123-EFG456"
  }
}
```

---
*Authorized by: Senior Systems Architect*
