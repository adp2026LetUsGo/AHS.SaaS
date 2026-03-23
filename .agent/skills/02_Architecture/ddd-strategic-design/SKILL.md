---
name: ddd-strategic-design
description: >
  Expert guidance on Domain-Driven Design strategic patterns for C1 Architect role in AHS.
  Covers Bounded Context definition, Context Mapping, Ubiquitous Language, Aggregate design,
  Domain Events, and translating business domains into AHS Cell specifications. Use this
  skill whenever the user mentions DDD, Bounded Context, Ubiquitous Language, Context Map,
  Aggregate, domain model, strategic design, core domain, supporting domain, generic subdomain,
  Anti-Corruption Layer, Shared Kernel, or defining a new AHS Cell from a business domain.
  Trigger on: DDD, Bounded Context, Ubiquitous Language, Context Map, Aggregate design,
  core domain, subdomain, ACL, Anti-Corruption Layer, domain events, domain model,
  new cell from business domain.
---

# DDD Strategic Design — C1 Architect Reference
## AHS Ecosystem / Blueprint V3.1

---

## 1. Subdomain Classification (decide this first)

Before defining any Cell, classify its subdomain. This determines investment level.

| Type | Definition | AHS Examples | Investment |
|---|---|---|---|
| **Core Domain** | Your competitive advantage. What you do better than anyone. | ColdChain Oracle (REQ-001), GxP Ledger | Maximum. Custom, own IP. |
| **Supporting Domain** | Enables core, but not unique to you. | AssetManager, FinTracker | Build lean. Could buy, chose to build for control. |
| **Generic Subdomain** | Commodity — identical everywhere. | Identity (Entra ID), Payments (Stripe), Email | Buy or use SaaS. Never build. |

```
AHS Cell decision rule:
  Core Domain     → AHS.Cell.[Name] — full Cell, full investment, full IP
  Supporting Domain → AHS.Cell.[Name] — Cell, but lean MVP first
  Generic Subdomain → DO NOT create a Cell. Integrate via Adapter/Port.
```

---

## 2. Bounded Context Definition Template

Use this template when C1 defines a new Cell for C2:

```markdown
## Bounded Context: [Name]

### Ubiquitous Language
[The shared vocabulary for THIS context ONLY.
Same word can mean different things in different contexts — specify here.]

| Term | Meaning in this context | Different from |
|---|---|---|
| "Asset" | Physical equipment with maintenance lifecycle | In FinTracker: "Asset" = financial instrument |
| "Status" | Active / Maintenance / Retired / Scrapped | In ColdChain: "Status" = Compliant / NonCompliant |
| "Event" | Domain event (AssetRetired) | NOT a calendar event |

### What this context OWNS (its truth)
- [What data this context is the single source of truth for]
- [What decisions only this context makes]

### What this context DOES NOT OWN
- [Data owned by other contexts — it only holds a reference ID]
- [Decisions deferred to other contexts]

### Core Aggregates
- [AggregateName]: [business purpose in one sentence]
  - Key invariants: [business rules that can NEVER be violated]
  - Lifecycle: [Created → ... → Final state]

### Domain Events (what this context announces to the world)
- [EventName]: when [business condition], consumed by [other contexts]

### Integration points (what it receives from others)
- From [ContextName]: [EventName] → [what we do with it]
```

---

## 3. Context Mapping — Inter-Cell Relationships

Map how AHS Cells relate to each other before C2 designs the integration:

### Relationship Patterns

```
PARTNERSHIP — both teams coordinate, evolve together
  AHS.Cell.ColdChain ←→ AHS.Cell.AssetManager
  (excursion events affect asset status)

CUSTOMER-SUPPLIER — one context depends on another's API
  AHS.Cell.ControlTower (customer) ← AHS.Cell.ColdChain (supplier)
  Supplier must honor consumer's needs. Consumer cannot change supplier's model.

CONFORMIST — downstream adopts upstream's model as-is (no translation)
  AHS.Cell.ShopifyBridge → Shopify API
  We conform to Shopify's model. No ACL needed.

ANTI-CORRUPTION LAYER (ACL) — translate foreign model to our model
  External SCADA system → [ACL Adapter] → AHS.Cell.ColdChain Domain
  The ACL prevents the SCADA model from polluting our domain.
  In code: lives in AHS.Cell.ColdChain.Infrastructure.Adapters

OPEN HOST SERVICE — publish a well-defined API others can consume
  AHS.Cell.ColdChain.Contracts → [Service Bus] → any Cell
  The Contracts project IS the Open Host Service.

PUBLISHED LANGUAGE — shared event schema agreed by all consumers
  ICellEvent interface + record types in AHS.Cell.[Name].Contracts
```

### AHS Context Map (current)

```
┌─────────────────────────────────────────────────────┐
│                 AHS Control Tower                    │
│            (Customer of all Cells)                   │
└──────┬──────────────┬──────────────┬────────────────┘
       │ Customer-    │ Customer-    │ Customer-
       │ Supplier     │ Supplier     │ Supplier
       ▼              ▼              ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│  ColdChain   │ │ AssetManager │ │  FinTracker  │
│  (Core)      │ │ (Supporting) │ │ (Supporting) │
└──────┬───────┘ └──────┬───────┘ └──────────────┘
       │ Partnership     │ subscribes to
       │ (events)        │ ColdChain events
       ▼                 │
┌──────────────┐         │
│  ShopifyBrdg │◄────────┘
│ (Conformist) │    [future]
└──────────────┘
       │ ACL (translates Shopify model)
       ▼
  Shopify API (Generic Subdomain — external)
```

---

## 4. Aggregate Design Rules (C1 specifies, C2 implements)

When C1 defines aggregates, follow these rules to avoid C2 redesign:

### Rule 1 — Small aggregates
```
❌ One giant aggregate: Order contains Customer, Products, Payments, Shipment
✅ Separate aggregates: Order, Customer, Payment — linked by ID only

Why: Large aggregates = large transactions = concurrency conflicts = performance issues
AHS Rule: If an aggregate has more than 5 direct child entities, split it.
```

### Rule 2 — Reference by ID across aggregates
```csharp
// ❌ Aggregate holding reference to another aggregate
public record Shipment
{
    public Asset Asset { get; init; }  // ← owns another aggregate
}

// ✅ Reference by ID only
public record Shipment
{
    public Guid AssetId { get; init; }  // ← just the ID
}
// If you need Asset data in Shipment queries → read model projection
```

### Rule 3 — Invariants = aggregate boundaries
```
An aggregate boundary = the scope of a single transaction.
Everything that must be consistent TOGETHER lives in one aggregate.
Everything that can be eventually consistent lives in separate aggregates.

Example:
- Shipment + its ExcursionList: consistent together → same aggregate ✅
- Shipment + the Asset it carries: eventually consistent → separate aggregates ✅
```

### Rule 4 — Factory methods carry the "why"
```csharp
// ❌ Constructor — no business meaning, no validation
new Shipment(id, tenantId, "Active", ...);

// ✅ Factory method — reads like the domain
Shipment.Create(
    cargo: CargoType.Pharmaceutical,
    route: RouteId.From("BCN-FRA-001"),
    insulation: InsulationType.Active,
    createdBy: actor);
// The factory method name IS the business operation
```

---

## 5. Domain Event Design (C1 specifies names and triggers)

```
Naming rule: [Noun][PastTense] — what happened, not what to do
  ✅ ShipmentCreated, ExcursionDetected, AssetRetired, PaymentReceived
  ❌ CreateShipment, DetectExcursion, RetireAsset (those are commands, not events)

Event carries: what happened + who caused it + when + enough data for consumers
Event does NOT carry: instructions for what to do next (that's the consumer's decision)
```

```csharp
// C1 specifies this level of detail:
// Event: ShipmentExcursionDetected
// Trigger: when temperature leaves setpoint range for longer than alarm delay
// Consumers: AssetManager (flag asset at risk), FinTracker (insurance trigger), Control Tower (alert)
// Data needed: ShipmentId, TenantSlug, ZoneId, ObservedCelsius, ExcursionStart, Severity

// C2 translates to:
public record ShipmentExcursionDetected(
    Guid   ShipmentId,
    string TenantSlug,
    string ZoneId,
    double ObservedCelsius,
    DateTimeOffset ExcursionStart,
    ExcursionSeverity Severity
) : ICellEvent;
```

---

## 6. Ubiquitous Language Anti-Patterns

Patterns that indicate a language problem — fix before C2 designs:

| Anti-Pattern | Signal | Fix |
|---|---|---|
| Generic names | "Item", "Entity", "Record", "Object", "Manager" | Name it by what it IS in the domain |
| CRUD language | "Create/Update/Delete Asset" | "Register Asset", "Schedule Maintenance", "Retire Asset" |
| Technical leakage | "Insert into assets table", "Call the API" | "Register an Asset" (domain language only) |
| Same word, different meanings | "Status" means 3 different things | Qualify: "ShipmentStatus", "AssetStatus", "ExcursionStatus" |
| Missing domain concepts | Developers invent a name for something the business calls something else | Interview the domain expert, find the real term |

---

## 7. C1 → C2 Domain Specification Checklist

Before handing off to C2, verify:

```
□ Subdomain type defined (Core / Supporting / Generic)
□ Ubiquitous Language glossary written (minimum 10 terms)
□ Context boundaries explicit (what this Cell owns vs references by ID)
□ All aggregates named with factory method intent
□ All domain events named in [Noun][PastTense] format
□ Event consumers identified for each event
□ Integration points mapped (Context Map pattern per relationship)
□ Anti-Corruption Layers identified for external systems
□ No CRUD language in the spec ("create/update/delete" → domain verbs)
□ Performance-sensitive paths flagged (C2 will apply ValueTask/Span)
□ Regulatory scope explicit (which events need GxP SignedCommand)
```
