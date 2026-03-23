---
name: c4-documentation-standard
description: >
  Expert guidance on the C4 Model for AHS documentation: Context, Containers, Components,
  and Code diagrams using Mermaid and PlantUML. Use this skill whenever the user mentions
  C4 model, C4 diagram, C4 Level 1, C4 Level 2, C4 Level 3, C4 Level 4, Context diagram,
  Container diagram, Component diagram, System architecture diagram, PlantUML C4, Mermaid
  architecture, architecture documentation, ADR, Architecture Decision Record, or asks
  to document a cell, system, or integration.
  Trigger on: C4, C4 model, context diagram, container diagram, component diagram,
  architecture diagram, PlantUML, Mermaid architecture, ADR, architecture decision.
---

# C4 Documentation Standard — AHS Ecosystem

## C4 Ownership in AHS

| Level | Name | Owner | Output |
|---|---|---|---|
| **L1** | System Context | C1 Architect | Who uses AHS? What external systems? |
| **L2** | Container | C1 Architect | What deployable units exist? How do they communicate? |
| **L3** | Component | C2 Engineer | What's inside each container? |
| **L4** | Code | AG (generated) | Actual classes, records, interfaces |

---

## 1. Level 1 — System Context (C1 Architect)

```mermaid
C4Context
  title AHS Ecosystem — System Context (L1)

  Person(operator, "Cold Chain Operator", "Monitors shipments,<br/>resolves excursions")
  Person(qa, "Quality Officer", "Approves, seals, signs<br/>GxP decisions")
  Person(admin, "Tenant Admin", "Manages users,<br/>cell configuration")

  System(ahs, "AHS Control Tower", "Universal Micro-SaaS platform.<br/>Orchestrates all Cells.")

  System_Ext(entra, "Microsoft Entra ID", "Identity & claims-based auth")
  System_Ext(servicebus, "Azure Service Bus", "Inter-cell event bus")
  System_Ext(sensors, "Customer Sensor Gateway", "MQTT / HTTP / OPC-UA<br/>(customer-owned)")
  System_Ext(shopify, "Shopify", "E-commerce integration<br/>(ShopifyBridge Cell)")
  System_Ext(erp, "Customer ERP", "SAP / D365 integration")

  Rel(operator, ahs, "Uses", "HTTPS / Blazor")
  Rel(qa, ahs, "Signs & seals", "HTTPS / Blazor")
  Rel(admin, ahs, "Configures", "HTTPS / Blazor")
  Rel(ahs, entra, "Authenticates via", "OIDC")
  Rel(sensors, ahs, "Sends readings to", "HTTP POST / Service Bus")
  Rel(ahs, shopify, "Syncs via", "Shopify REST API")
  Rel(ahs, erp, "Integrates via", "Adapter/Port pattern")
  Rel(ahs, servicebus, "Publishes/Subscribes", "AMQP")
```

---

## 2. Level 2 — Containers (C1 Architect)

```mermaid
C4Container
  title AHS Ecosystem — Containers (L2)

  Person(user, "AHS User")

  Container_Boundary(ahs, "AHS Ecosystem") {
    Container(ui, "AHS.Web.UI", "Blazor / .NET 10", "Control Tower.<br/>Sovereign Elite UI.")
    Container(cell_cc, "Cell: ColdChain API", "Minimal API / Native AOT", "Temperature monitoring,<br/>excursion detection, Oracle.")
    Container(cell_am, "Cell: AssetManager API", "Minimal API / Native AOT", "Asset lifecycle,<br/>maintenance, GxP audit.")
    Container(cell_ft, "Cell: FinTracker API", "Minimal API / Native AOT", "Financial tracking,<br/>multi-currency.")
    Container(cell_sb, "Cell: ShopifyBridge API", "Minimal API / Native AOT", "Shopify sync,<br/>order management.")
    ContainerDb(db_cc, "DB: ColdChain", "PostgreSQL 17", "Append-only ledger,<br/>sensor data, excursions.")
    ContainerDb(db_am, "DB: AssetManager", "PostgreSQL 17", "Asset records,<br/>maintenance logs.")
    ContainerDb(redis, "Redis", "Redis 7", "HybridCache L2<br/>for all cells.")
    Container(bus, "Azure Service Bus", "AMQP", "Inter-cell events.<br/>sensor-readings topic.")
    Container(kv, "Azure Key Vault", "HSM", "GxP HMAC keys,<br/>connection strings.")
  }

  Rel(user, ui, "Uses", "HTTPS")
  Rel(ui, cell_cc, "Calls", "HTTP/gRPC")
  Rel(ui, cell_am, "Calls", "HTTP/gRPC")
  Rel(cell_cc, db_cc, "Reads/Writes", "Npgsql")
  Rel(cell_am, db_am, "Reads/Writes", "Npgsql")
  Rel(cell_cc, bus, "Publishes events", "AMQP")
  Rel(cell_am, bus, "Subscribes", "AMQP")
  Rel(cell_cc, redis, "Caches", "StackExchange.Redis")
  Rel(cell_cc, kv, "Reads secrets", "Managed Identity")
```

---

## 3. Level 3 — Components (C2 Engineer)

```mermaid
C4Component
  title AHS.Cell.ColdChain.API — Components (L3)

  Container_Boundary(api, "AHS.Cell.ColdChain.API") {
    Component(ep, "ShipmentEndpoints", "Minimal API MapGroup", "REST endpoints for<br/>shipment CRUD + Oracle.")
    Component(oracle_ep, "OracleEndpoints", "Minimal API MapGroup", "POST /oracle/calculate<br/>returns OracleResult + XAI DNA.")
    Component(cmd, "ShipmentCommandHandler", "CQRS Handler", "Processes SignedCommands.<br/>Writes to EventStore.")
    Component(qry, "ShipmentQueryHandler", "CQRS / Dapper", "Read-side queries.<br/>Zero EF overhead.")
    Component(oracle, "LogisticsOracle", "Domain Service", "REQ-001 inference.<br/>P99 < 10ms. ValueTask.")
    Component(ledger, "GxPLedger", "Infrastructure", "SHA256 hash chain.<br/>HMAC-sealed entries.")
    Component(db, "ColdChainDbContext", "EF Core 10", "Write-side persistence.<br/>Source Gen. AOT-safe.")
    Component(adapter, "SensorIngestAdapter", "Infrastructure", "ISensorAdapter impl.<br/>Protocol-agnostic.")
    Component(pub, "CellEventPublisher", "Infrastructure", "Publishes ICellEvent<br/>to Service Bus.")
  }

  Rel(ep, cmd, "Dispatches to")
  Rel(ep, qry, "Queries via")
  Rel(oracle_ep, oracle, "Invokes")
  Rel(cmd, ledger, "Seals events in")
  Rel(cmd, db, "Persists via")
  Rel(cmd, pub, "Publishes via")
  Rel(adapter, pub, "Normalizes & publishes")
```

---

## 4. Level 4 — Code (AG generates)

```
L4 is the actual generated code. C2 produces the Prompt Maestro for AG.
AG generates following the Cell Template (see: ahs-cell-template skill).

Document L4 as code comments in the generated files:
```

```csharp
/// <summary>
/// C4 L4 — ShipmentCommandHandler
/// Layer: Application
/// Cell: AHS.Cell.ColdChain
/// Responsibility: Process all state-changing commands for Shipment aggregate.
/// Dependencies: IEventStore, IGxPLedger, ICellEventPublisher
/// Pattern: CQRS Command Handler + GxP Electronic Signature
/// </summary>
public class ShipmentCommandHandler(
    IEventStore store,
    IGxPLedger ledger,
    ICellEventPublisher publisher)
{ }
```

---

## 5. Architecture Decision Records (ADR)

```markdown
# ADR-001: Database-per-Cell over Shared Schema

**Status**: Accepted
**Date**: 2025-Q1
**Deciders**: C1 Architect

## Context
AHS cells must be independently deployable and sellable as standalone Micro-SaaS.

## Decision
Each Cell owns its own PostgreSQL database (Database-per-Cell pattern).
Cross-cell data exchange via Service Bus events only. No direct DB joins.

## Consequences
+ Full cell autonomy — deploy, scale, sell independently
+ GxP compliance per cell (each DB has its own RLS + DENY policies)
+ Simpler authorization (tenant RLS per DB, not per table)
- No cross-cell JOINs (mitigated by read model projections)
- Higher infrastructure cost (multiple DBs) — offset by serverless tier

## Alternatives Rejected
- Schema-per-tenant in shared DB: violates cellular isolation
- Shared DB with discriminator: impossible to sell cells independently
```

```markdown
# ADR-002: Native AOT as Default Compilation Target

**Status**: Accepted
**Date**: 2025-Q1
**Deciders**: C1 Architect

## Context
AHS cells are deployed as Azure Container Apps with scale-to-zero.
Cold start latency directly impacts user experience and cost.

## Decision
All Cell APIs publish as Native AOT (PublishAot=true, linux-x64).
Target: cold start < 50ms, image size < 80MB.

## Consequences
+ Sub-50ms cold starts on scale-to-zero
+ ~35MB images (vs 200MB+ with full runtime)
- No reflection: requires JsonSerializerContext, Mapperly, manual DI
- No Assembly.Load(), dynamic, Expression compilation
- EF Core: no lazy loading, explicit query filters per entity

## Implementation
All cells: PublishAot=true in csproj.
CI gate: IL trim warnings treated as errors (IL2026, IL3050).
```

---

## 6. Mermaid C4 Quick Reference

```mermaid
%%{init: {'theme': 'dark'}}%%
C4Context
  %% Personas
  Person(id, "Label", "Description")
  Person_Ext(id, "External User", "Description")

  %% Systems
  System(id, "System Name", "Description")
  System_Ext(id, "External System", "Description")
  SystemDb(id, "Database", "Description")
  SystemQueue(id, "Queue", "Description")

  %% Relationships
  Rel(from, to, "Label", "Technology")
  Rel_Back(from, to, "Label")
  Rel_Neighbor(from, to, "Label")
  BiRel(a, b, "Label")

  %% Boundaries
  Boundary(id, "Label", "type") { ... }
  Enterprise_Boundary(id, "Label") { ... }
  System_Boundary(id, "Label") { ... }
  Container_Boundary(id, "Label") { ... }
```

---

## 7. PlantUML C4 (alternative for VS2026)

```plantuml
@startuml AHS-L2
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

LAYOUT_WITH_LEGEND()

title AHS Ecosystem — Container Diagram (L2)

Person(user, "AHS Operator")
System_Ext(entra, "Entra ID")

System_Boundary(ahs, "AHS Ecosystem") {
  Container(ui, "AHS.Web.UI", "Blazor/.NET 10", "Control Tower")
  Container(api, "Cell API", "Minimal API/AOT", "Domain cell")
  ContainerDb(db, "Cell DB", "PostgreSQL 17", "Isolated per cell")
  Container(bus, "Service Bus", "AMQP", "Inter-cell events")
}

Rel(user, ui, "Uses", "HTTPS")
Rel(ui, api, "Calls", "HTTP")
Rel(api, db, "Reads/Writes", "Npgsql")
Rel(api, bus, "Publishes", "AMQP")
Rel(ui, entra, "Auth via", "OIDC")

@enduml
```
