# PRD: AHS Xinfer Cell
## "Active Defense System for Pharmaceutical Cold Chain"

**Version**: 3.0 (V3.1 refactor of V2.0)  
**Status**: In Development  
**Cell namespace**: AHS.Cell.Xinfer  
**Standalone product name**: AHS ColdGuard

---

### 1. The Problem
Pharmaceutical companies lose €50K–€2M per excursion event when they cannot prove regulatory compliance. Current solutions fail at three points:
- No predictive risk (reactive only)
- Manual, error-prone audit trails
- Black-box algorithms (non-explainable)

---

### 2. Solution — Standalone Value Proposition
- **Active prediction**: Logistics Oracle calculates Pessimistic TTF before excursion
- **Explainable AI**: 14-point XAI DNA for every risk decision
- **Immutable compliance**: SHA256-sealed GxP Ledger

Tagline: *"From passive logging to active defense."*

---

### 3. Target Personas
- **Primary Buyer**: VP Quality / QA Director  
- **Primary User**: Cold Chain Operator  
- **Regulatory Stakeholder**: FDA Inspector / QA Auditor

---

### 4. Features
#### P0 — MVP
| ID | Feature | User Story | Acceptance Criteria |
|---|---|---|---|
| F-001 | Shipment lifecycle | Track shipments | Full GxP audit trail |
| F-002 | Excursion detection | Immediate alerts | Severity classified |
| F-003 | Logistics Oracle | Risk assessment pre-dispatch | P99 < 10ms; 14-factor XAI |
| F-004 | GxP Ledger | Tamper-evident records | PDF/CSV + hash |
| F-005 | What-If Simulator | Model route changes | ReasonForChange sealed |
| F-006 | MKT Report | Mean Kinetic Temperature | ICH Q1A-compliant |
| F-007 | Multi-protocol ingestion | Use existing sensors | Webhook + 1 protocol |
| F-008 | Tenant management | Configure zones/setpoints | Per-tenant validation |

#### P1 — Growth
| ID | Feature | Rationale |
|---|---|---|
| F-101 | Stability Budget calculator | Cumulative excursion budget per product |
| F-102 | Route risk library | Pre-configured Oracle risk profiles |
| F-103 | Carrier scorecard | Track carrier reliability over time |
| F-104 | Regulatory export templates | FDA, EMA, COFEPRIS |
| F-105 | API for LIMS integration | Push data to existing LIMS |

#### P2 — Delight
| ID | Feature | Rationale |
|---|---|---|
| F-201 | Predictive maintenance alerts | Correlate excursions with carrier incidents |
| F-202 | Benchmarking dashboard | Compare carrier performance |
| F-203 | Mobile-first incident response | Field operator app |

#### Out of Scope
- Asset management → AssetManager Cell
- Financial tracking → FinTracker Cell
- Carrier payments → Generic subdomain (Stripe)

---

### 5. Domain Model
| Aggregate | Business Role | Key Invariants |
|---|---|---|
| Shipment | Cargo movement | Cannot seal without CCPs resolved |
| TemperatureZone | Controlled environment | MinCelsius < MaxCelsius |
| Sensor | Registered monitoring device | One Sensor → one Zone |

---

### 6. Regulatory Scope
| Regulation | Status | Key Requirement |
|---|---|---|
| FDA 21 CFR Part 11 | Mandatory | SignedCommand + SHA256 Ledger |
| EU GMP Annex 11 | Mandatory | ALCOA+ compliance |
| HACCP | Mandatory | CCP config, corrective actions |
| GDPR | Conditional | PII separated per tenant |

---

### 7. Success Metrics
| Metric | Target |
|---|---|
| Oracle P99 latency | < 10ms |
| Sensor ingestion P99 | < 50ms |
| FDA audit export | < 5s |
| System availability | 99.9% |
| Excursion false positive rate | < 2% |