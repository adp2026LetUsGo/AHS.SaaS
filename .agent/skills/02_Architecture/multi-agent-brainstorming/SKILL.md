---
name: multi-agent-brainstorming
description: >
  High-stakes design validation for AHS Ecosystem decisions. Use this skill when a design
  is high-impact, high-risk, or requires elevated confidence before C2 writes the Prompt
  Maestro for AG. Simulates a panel of three expert reviewers (Architect, Domain Expert,
  Devil's Advocate) who challenge the design from different angles. Trigger when:
  brainstorming skill flags high-impact handoff, designing a new AHS Cell from scratch,
  making an ADR-level architectural decision, changing a Blueprint guardrail, designing
  cross-cell integration, or any decision that affects GxP compliance or Native AOT
  compatibility across multiple Cells.
  Trigger on: high-impact design, new Cell, ADR, cross-cell, GxP architecture,
  multi-agent review, design validation, elevated confidence, panel review.
risk: low
source: ahs-core
date_added: '2026-03-19'
---

# Multi-Agent Brainstorming — AHS High-Impact Design Validation

## When This Skill Activates

This skill is the mandatory next step when `brainstorming` outputs a finalized design
marked as high-impact, high-risk, or requiring elevated confidence.

**Always required for:**
- New AHS Cell definition (first Prompt Maestro for a domain)
- Any ADR that changes a Blueprint V3.1 guardrail
- Cross-cell integration designs (Outbox, Saga, new Service Bus topic)
- GxP compliance architecture (new SignedCommand flow, Ledger schema change)
- Performance-critical design (P99 targets, SIMD engine, Oracle hot path)
- Control Tower BFF changes (new real-time widget, new SignalR hub)
- Tenant isolation changes (IsolationMode, schema migration strategy)

**Not required for:**
- Incremental feature additions to existing Cells (use `brainstorming` only)
- UI component additions to AHS.Web.Common
- Test additions or coverage improvements
- Documentation updates

---

## The Panel

Three expert personas review the design simultaneously and independently.
Each has a fixed agenda — they do not agree with each other by default.

---

### Persona 1 — The Architect (C1 lens)

**Agenda:** Strategic coherence and long-term viability.

Reviews for:
- Does this design align with Blueprint V3.1 and the Cellular Manifesto?
- Does the namespace follow `AHS.Cell.[Name].[Layer]`?
- Is this the right subdomain classification (Core / Supporting / Generic)?
- Does Database-per-Cell hold? No cross-cell SQL joins?
- Is inter-cell communication exclusively via Service Bus?
- Is the C4 model (L1-L2) consistent with this design?
- Will this Cell be independently sellable as Micro-SaaS?
- Does the Ubiquitous Language avoid CRUD terms?

**Output format:**
```
ARCHITECT REVIEW:
✅ Aligned: [what is correct]
⚠️  Concern: [what needs attention]
❌ Violation: [what breaks a Blueprint guardrail]
Recommendation: [specific action]
```

---

### Persona 2 — The Domain Expert (C2 lens)

**Agenda:** Technical correctness and implementability.

Reviews for:
- Is this design Native AOT compatible? (no reflection, source gen everywhere)
- Does every write command inherit `SignedCommand`?
- Is the EF Core vs Dapper split correct? (write = EF Core, read = Dapper)
- Are aggregates small enough? (>5 direct children = split signal)
- Does the GxP Ledger cover all state-changing operations?
- Are P99 targets achievable with the proposed design? (Oracle < 10ms)
- Is `ValueTask` used correctly in hot paths?
- Are `Span<T>` / `stackalloc` used where LINQ would cause allocations?
- Does the `JsonSerializerContext` cover all types crossing the API boundary?
- Will NetArchTest pass? (Domain zero deps, commands inherit SignedCommand)

**Output format:**
```
DOMAIN EXPERT REVIEW:
✅ Implementable: [what works]
⚠️  Risk: [what may cause issues in implementation]
❌ Blocker: [what will fail — AOT, NetArchTest, performance target]
Recommendation: [specific code pattern or structural change]
```

---

### Persona 3 — The Devil's Advocate (stress tester)

**Agenda:** Find what the other two missed. Assume the design will fail.

Challenges:
- What happens when the first enterprise tenant demands physical isolation?
  Does `IsolationMode` handle this without code changes?
- What happens when Cell B is down and Cell A publishes an event?
  Does the Outbox Pattern cover this, or is there data loss?
- What happens when AG generates this Cell in 6 months and the developer
  has no context? Is Section 0 of the Prompt Maestro explicit enough?
- What is the failure mode if the GxP Ledger hash chain breaks mid-migration?
- What if the SIMD engine receives NaN or infinity in a temperature reading?
- What if a customer's sensor sends readings 10x faster than expected?
  Does the `Channel<T>` bounded capacity drop readings or backpressure?
- What if Entra ID is unavailable for 5 minutes? Can operators still work?
- What is the GDPR right-to-erasure strategy for this Cell specifically?

**Output format:**
```
DEVIL'S ADVOCATE CHALLENGES:
Challenge 1: [scenario]
  Worst case: [what breaks]
  Current design handles this: YES / NO / PARTIALLY
  If NO: [required design change]

Challenge 2: ...
```

---

## The Process

### Step 1 — Receive the Design Package

Accept the output from `brainstorming`:
- Understanding summary
- Assumptions list
- Decision log
- Final design (architecture, components, data flow)

If any of these are missing, **stop and request them**.
Do not run the panel on an incomplete design.

---

### Step 2 — Run the Panel (Sequentially)

Present each persona's review in full before moving to the next.
Do not merge or summarize mid-review — each voice must be heard completely.

```
─── ARCHITECT REVIEW ──────────────────────────────────────────
[full Architect review]

─── DOMAIN EXPERT REVIEW ──────────────────────────────────────
[full Domain Expert review]

─── DEVIL'S ADVOCATE CHALLENGES ───────────────────────────────
[full Devil's Advocate challenges]
```

---

### Step 3 — Synthesis

After all three reviews, produce a synthesis:

```
PANEL SYNTHESIS:

Consensus (all three agree):
  - [point 1]
  - [point 2]

Conflicts (reviewers disagree):
  - [topic]: Architect says X, Domain Expert says Y → recommended resolution

Critical blockers (must fix before Prompt Maestro):
  - [blocker 1] → required change
  - [blocker 2] → required change

Non-blocking concerns (fix in v1.1 or document as known risk):
  - [concern 1]

Design verdict: APPROVED / APPROVED WITH CONDITIONS / REJECTED
```

---

### Step 4 — Resolution Loop

If verdict is **APPROVED WITH CONDITIONS**:
- List the required changes explicitly
- Re-run only the affected personas after changes are made
- Do not re-run the full panel unless a critical blocker was found

If verdict is **REJECTED**:
- Return to `brainstorming` with the panel's findings as constraints
- Do not proceed to Prompt Maestro

If verdict is **APPROVED**:
- Produce the **Updated Decision Log** incorporating panel findings
- Hand off to C2 with: design package + decision log + any open risks documented

---

### Step 5 — Prompt Maestro Authorization

Only after panel verdict is **APPROVED** or **APPROVED WITH CONDITIONS** (resolved):

```
PROMPT MAESTRO AUTHORIZATION:

Cell: [CellName]
Panel date: [date]
Verdict: APPROVED
Critical blockers resolved: YES / N/A
Open risks documented: [list or NONE]

C2 may now write the Prompt Maestro for AG.
Reference this authorization in Prompt Maestro Section 0.
```

---

## AHS-Specific Challenge Bank

The Devil's Advocate draws from this bank for every review.
Add new challenges as the ecosystem evolves.

### Multitenancy
- New enterprise tenant requires physical DB isolation → `IsolationMode.Isolated` path tested?
- GDPR data residency: which Azure region for this tenant's schema?
- Tenant onboarding script: creates schema + runs migrations + updates registry atomically?

### Native AOT
- Any `JsonSerializer.Serialize(obj)` without `JsonSerializerContext`?
- Any `Activator.CreateInstance`, `Assembly.GetTypes`, `BindingFlags`?
- Any library dependency that uses Castle DynamicProxy or Expression.Compile?
- CI trim warning gate configured? (`IL2026`, `IL3050` as errors)

### GxP Integrity
- Every write command inherits `SignedCommand`? (`ReasonForChange` validated in constructor)
- GxP Ledger `REVOKE UPDATE, DELETE` on PostgreSQL table?
- SHA256 hash chain verified in integration tests?
- Audit export endpoint (PDF + CSV) implemented?

### Performance
- Oracle hot path: `ValueTask`, no LINQ, no `$""` interpolation?
- Sensor ingestion: `Channel<T>` bounded? What happens when full?
- Dapper queries: `set_config` called before every query? (tenant RLS)
- HybridCache TTL defined for every cached type?

### Cross-Cell
- Service Bus topic + subscription configured in docker-compose AND bicep?
- Outbox Pattern implemented? (DB + Service Bus atomic)
- Consumer is idempotent? (Service Bus delivers at-least-once)
- Dead letter queue monitored? (Azure Monitor alert configured)

### Sovereign Elite UI
- All glass surfaces use `<GlassCard>` / `<GlassPanel>` from AHS.Web.Common?
- All command forms include `<ReasonForChangeModal>`?
- Zero hardcoded hex colors in .razor files?
- New components added to AHS.Web.Common, not defined inline?
