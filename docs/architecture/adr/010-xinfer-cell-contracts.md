# Architecture Decision Record (ADR) 010: Xinfer Cell Contracts and Commercial Isolation

## Status
Accepted

## Context
The Xinfer Cell operates as a highly specialized Autonomous Cell within the AHS Ecosystem. Its primary responsibility is executing the thermal inference risk engine (eXplainable AI). While it integrates deeply with other internal logistics operations, its capabilities have immense value outside of our primary internal usage.

## Decision
We mandate strict commercial and technical isolation of the Xinfer Cell via **Contract-Based Communication** and the **Standard Envelope Pattern**.

### 1. Versioned Contracts as the Technical Boundary
Xinfer will expose its inbound and outbound data structures solely through versioned contracts (e.g., `InferenceInput_v1`, `InferenceOutput_v1`). Consumers, whether internal AHS components or external B2B clients, will only bind to these `.Contracts` packages. 

### 2. The Standard Envelope
Every single interaction must be wrapped in a `StandardEnvelope`, providing uniform error handling (`status`) and GxP traceability (`metadata`).

### 3. Commercial Extraction (Micro-SaaS)
This isolation is not merely technical. By decoupling Xinfer from the core AHS monolith database and routing entirely through standard contracts, we enable **Commercial Extraction**. Xinfer can be lifted from the AHS Control Tower, deployed on isolated infrastructure, and sold as an independent, standalone Micro-SaaS to third-party logistics providers.

## Consequences

### Positive:
- **Resilience:** Internal systems do not crash if Xinfer logic changes, as long as the contract `v1` is honored.
- **Monetization:** Xinfer is ready to be sold as an independent API product without refactoring dependencies.
- **Security:** Strict boundaries mean external clients only access the inference engine, not our operational data.

### Negative:
- Strict contract versioning overhead when releasing new ML model parameters.
- Maintaining separate CI/CD pipelines to guarantee its standalone deployability.
