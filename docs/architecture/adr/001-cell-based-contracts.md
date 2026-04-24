# Architecture Decision Record (ADR) 001: Cell-Based Contracts and Isolation for Xinfer

## Status
Accepted

## Context
The AHS Ecosystem (V3.1) relies heavily on the "Cellular Architecture" pattern, where each Bounded Context is designed as an autonomous "Cell" (a Micro-SaaS unit). The `Xinfer` Cell acts as an inference engine for logistics risk (XAI - eXplainable AI). 
We need to establish a resilient, decoupled, and secure communication mechanism between the Xinfer Cell and external consumers (such as UI Demos or the AHS Control Tower), while ensuring strict adherence to GxP and zero-logic presentation layers.

## Decision
We have decided to implement strict **Contract-Based Communication** combined with **Cell Isolation** for the Xinfer ecosystem.

### Key Principles:
1. **Standard Envelope Pattern:** All inbound and outbound communication with the Xinfer Cell must be wrapped in a `StandardEnvelope`. This envelope enforces a strict structure:
   - `metadata`: Contains `cell_id`, `contract_version`, and timestamps for traceability.
   - `data`: The actual payload (e.g., `InferenceOutput_v1`).
   - `status`: Execution status, error codes, and a `trace_id` to correlate cross-cell logs.
2. **Immutable Contracts:** Data Transfer Objects (DTOs) and event schemas (e.g., `InferenceInput_v1`, `InferenceOutput_v1`) are defined as versioned contracts (`.Contracts` project). The UI and other Cells consume these contracts directly, ensuring type safety without sharing domain logic.
3. **Database-per-Cell:** Xinfer maintains its own isolated persistence layer. Other cells cannot query Xinfer's database directly; they must interact via the established API contracts or listen to published Domain Events (Outbox Pattern).

## Consequences

### Positive:
- **Decoupling:** The UI (`AHS.Xinfer.UI.Demo`) is completely decoupled from the Xinfer backend, sharing only the TypeScript interfaces generated from the C# contracts.
- **Traceability (GxP):** The `StandardEnvelope` guarantees that every request and response carries a `trace_id` and `cell_id`, fulfilling the auditability requirements.
- **Predictable Error Handling:** Clients can rely on a consistent error structure. The frontend intercepts the `status` object to present unified business errors.
- **Independent Scalability:** Xinfer can be deployed, scaled, and updated independently as long as it honors the `v1` contracts.

### Negative / Trade-offs:
- **Overhead:** Wrapping every response in a standard envelope adds a slight payload overhead and requires boilerplate interception logic in the clients (e.g., Axios interceptors).
- **Versioning Complexity:** Modifying a contract requires a new version (`v2`) to prevent breaking existing consumers, increasing maintenance effort for legacy versions.

## References
- AHS Ecosystem Blueprint V3.1
- docs/RESUME_CONTEXT.md
