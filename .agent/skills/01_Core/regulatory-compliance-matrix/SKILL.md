---
name: regulatory-compliance-matrix
description: >
  Expert guidance on mapping regulatory requirements to AHS Cell design decisions for C1
  Architect. Covers FDA 21 CFR Part 11, EU GMP Annex 11, GDPR, HACCP, ISO 27001,
  PSD2, and ALCOA+ — translated into concrete product and architecture requirements that
  C1 hands to C2. Use this skill whenever the user mentions compliance, regulation,
  FDA, GxP, GDPR, HIPAA, HACCP, ISO 27001, electronic signatures, audit trail requirements,
  data residency, right to erasure, or asks what a regulation means for a Cell design.
  Trigger on: FDA, GxP, GDPR, HACCP, ISO 27001, HIPAA, PSD2, compliance, regulation,
  electronic signature, audit trail, data residency, right to erasure, ALCOA.
---

# Regulatory Compliance Matrix — C1 Architect Reference
## AHS Ecosystem / Blueprint V3.1

---

## Quick Routing Table

| Cell Domain | Primary Regulation | Secondary | GxP Required? |
|---|---|---|---|
| Pharmaceutical cold chain | FDA 21 CFR Part 11 + EU Annex 11 | ALCOA+, ICH Q10 | ✅ Mandatory |
| Food cold chain | HACCP / FDA Food Safety | FSMA, EU 852/2004 | ✅ Recommended |
| Chemical cold chain | ADR / IATA DGR | ISO 9001 | ✅ Recommended |
| Asset management | ISO 55001 | ISO 9001 | ⚠️ Depends on asset criticality |
| Financial tracking | PSD2 / MiFID II | SOX (if public) | ⚠️ Depends on jurisdiction |
| E-commerce (Shopify) | GDPR (EU customers) | PCI DSS (payments) | ❌ GxP not required |
| All Cells with EU users | GDPR | ePrivacy | ❌ GxP not required |
| All Cells | ISO 27001 (information security) | — | ❌ GxP not required |

---

## 1. FDA 21 CFR Part 11 — Electronic Records & Signatures

### What it requires (C1 product decisions)

| Requirement | Product Decision | C2 Implementation |
|---|---|---|
| **§11.10(a)** Validation | System must be validated for its intended use | Test suite coverage > 80%, Reqnroll BDD acceptance tests |
| **§11.10(b)** Legible copies | Export audit records in human-readable format | PDF/CSV export endpoint for GxP Ledger |
| **§11.10(c)** Record protection | Records cannot be deleted or modified | `REVOKE UPDATE, DELETE` + `ENABLE ROW LEVEL SECURITY` |
| **§11.10(e)** Audit trail | Who did what, when, and why | `SignedCommand` with `ReasonForChange` + SHA256 ledger |
| **§11.10(g)** Access control | Only authorized users can perform operations | Entra ID claims + `SameTenant` policy + `ahs_role` claim |
| **§11.50** Signature manifestations | E-signature must show: signer, date/time, meaning | `SignedByName` + `SignedAt` + `ReasonForChange` in ledger |
| **§11.70** Signature linking | Signature must be permanently linked to the record | HMAC seal links signature to ledger entry hash |

### What triggers FDA 21 CFR Part 11 in AHS
```
ALWAYS required:
  - Any Cell handling pharmaceutical products
  - Any quality decision (approve, reject, quarantine)
  - Any parameter change in the What-If Simulator
  - Any deviation or excursion record

C1 DECISION: Mark in the Cell PRD which operations are §11 operations.
C2 CONSEQUENCE: Those operations MUST use SignedCommand + GxP Ledger.
```

### ALCOA+ Checklist (for GxP Cell audit readiness)
```
A — Attributable:   Every record has ActorId + ActorName (who did it)
L — Legible:        JSON payload + human-readable EventType
C — Contemporaneous: OccurredAt = DateTimeOffset.UtcNow at event creation (UTC)
O — Original:        Append-only PostgreSQL + REVOKE UPDATE/DELETE
A — Accurate:        SHA256 hash chain + HMAC seal prevents tampering
+ Complete:          VerifyChain() on audit export — no gaps
+ Consistent:        UTC timestamps throughout, no timezone conversion
+ Enduring:          90-day Key Vault soft-delete, 1-year log retention (Log Analytics)
+ Available:         Read model projections for fast audit queries
```

---

## 2. GDPR — General Data Protection Regulation

### What it requires (C1 product decisions per Cell)

| Requirement | Product Decision | C2 Implementation note |
|---|---|---|
| **Art. 5** Data minimization | Only collect data necessary for the stated purpose | C1: define exactly what PII each Cell collects |
| **Art. 17** Right to erasure | User can request deletion of their personal data | C2: soft-delete + anonymization strategy per Cell |
| **Art. 20** Data portability | User can export their data | Export endpoint per Cell |
| **Art. 25** Privacy by design | Privacy built in, not added on | PII never in event payloads by default |
| **Art. 32** Security | Encryption at rest + in transit | TLS everywhere, PostgreSQL encryption at rest (Azure) |
| **Art. 33** Breach notification | 72h notification to authority | Azure Security Center + runbook |
| **Art. 35** DPIA | Required for high-risk processing (health data) | Pharmaceutical Cell requires DPIA document |

### GDPR Cell Design Rules (C1 mandates, C2 implements)

```
Rule 1 — PII never in Domain Events
  ❌ ShipmentCreated includes CustomerEmail, CustomerPhone
  ✅ ShipmentCreated includes CustomerId (GUID reference only)
  Reason: Domain events are stored forever in the GxP Ledger.
          Email addresses must be erasable — event payloads cannot be modified.

Rule 2 — Separate PII store per Cell
  Each Cell that handles PII has a separate table: pii_data(id, tenant_id, data_json)
  Domain events reference pii_data.id only.
  Right to erasure: DELETE from pii_data WHERE id = @id (event chain intact, PII gone)

Rule 3 — Data Residency
  EU customers: Azure West Europe or North Europe regions only
  Azure Flexible Server: set region in bicep per tenant geography

Rule 4 — Consent tracking
  If Cell collects PII beyond operational necessity → ConsentRecord aggregate
  ConsentRecord: ConsentId, SubjectId, Purpose, GrantedAt, RevokedAt, LawfulBasis
```

### GDPR Risk by Cell
```
🔴 HIGH — Pharmaceutical ColdChain (patient data, health records)
           → DPIA required, DPA agreement with Azure, explicit consent
🟡 MEDIUM — AssetManager (employee names as actors)
           → Legitimate interest basis, data minimization
🟡 MEDIUM — FinTracker (financial data, IBAN, tax IDs)
           → Contractual necessity basis, PCI DSS if card data
🟢 LOW    — ShopifyBridge (B2B, minimal personal data)
           → Standard privacy notice sufficient
```

---

## 3. HACCP — Hazard Analysis and Critical Control Points

### What it requires (C1 product decisions for Cold Chain Cell)

| HACCP Principle | Product Requirement | AHS Cell Feature |
|---|---|---|
| **P1** Hazard Analysis | Identify biological, chemical, physical hazards | ZoneProfile configuration per cargo type |
| **P2** Critical Control Points | Define CCPs (temperature checkpoints) | CCP entity in domain model |
| **P3** Critical Limits | Min/max temperature per CCP | Setpoint configuration + ExcursionDetector |
| **P4** Monitoring | Continuous monitoring at each CCP | Sensor ingestion pipeline + Channel<T> |
| **P5** Corrective Actions | Define what happens when limit is breached | ExcursionResolved workflow + SignedCommand |
| **P6** Verification | Verify the system works | MKT calculation + ColdChainReport |
| **P7** Record Keeping | Maintain records for inspection | GxP Ledger + export endpoint |

```
C1 DECISION for Cold Chain Cell:
  Every shipment has a HACCP Plan (cargo type + route + CCP list)
  Every CCP has Critical Limits (min/max celsius + alarm delay)
  Every excursion triggers a mandatory Corrective Action workflow
  Every shipment closure requires a Verification record (MKT + disposition)
  All records must be exportable for regulatory inspection
```

---

## 4. ISO 27001 — Information Security (All Cells)

### Mandatory controls for ALL AHS Cells

| Control | Requirement | Implementation |
|---|---|---|
| **A.9** Access control | Role-based access, least privilege | Entra ID + `ahs_role` claims + `SameTenant` policy |
| **A.10** Cryptography | Encryption of sensitive data | Key Vault + HMAC-SHA256 + TLS 1.3 |
| **A.12** Operations security | Logging, monitoring | Azure Monitor + structured telemetry |
| **A.13** Communications security | Network segmentation | Container Apps VNet integration |
| **A.14** System development | Secure development | NetArchTest + SAST in CI (GitHub Advanced Security) |
| **A.16** Incident management | Respond to security events | Azure Security Center + runbook |
| **A.18** Compliance | Monitor regulatory compliance | Compliance dashboard in Control Tower |

---

## 5. C1 Compliance Checklist per New Cell

```
□ Subdomain classified → determines which regulations apply
□ PII inventory: what personal data does this Cell collect?
□ Lawful basis for processing (consent / contract / legitimate interest)
□ GDPR: PII separated from event payloads?
□ GDPR: Right to erasure strategy defined?
□ GDPR: Data residency requirement (EU / US / global)?
□ GxP: Which operations require SignedCommand + Ledger?
□ GxP: Is a DPIA required? (health data → yes)
□ HACCP: Is this a food/pharma/chemical Cell? → HACCP plan required
□ ISO 27001: Key Vault secrets identified for this Cell
□ ISO 27001: Logging requirements defined (what events go to Azure Monitor)
□ Data retention policy defined (how long, where, who can access)
□ Regulatory body contact identified (FDA? AEMPS? ICO? AEPD?)
```

---

## 6. Regulation → C2 Requirement Translation

C1 produces this translation for each regulated Cell:

```markdown
## Compliance Requirements: [Cell Name]

### Applicable regulations
- [Regulation]: [specific articles/sections]

### GxP Operations (require SignedCommand + Ledger)
- [Operation]: [regulation article] → SignedCommand + Ledger entry
- [Operation]: [regulation article] → SignedCommand + Ledger entry

### PII Handling
- Data collected: [list]
- Lawful basis: [consent / contract / legitimate interest]
- Erasure strategy: [soft-delete / anonymization / pseudonymization]
- Retention period: [X years per regulation]

### Export Requirements
- Audit export format: [PDF / CSV / JSON]
- Retention: [X years]
- Accessible by: [Quality Officer / Regulator / Tenant Admin]

### Security Requirements
- Secrets in Key Vault: [list what secrets]
- Logging: [what events must reach Azure Monitor]
- Data residency: [Azure region constraint]
```
