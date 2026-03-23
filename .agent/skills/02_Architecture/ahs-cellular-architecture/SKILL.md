---
name: ahs-cellular-architecture
description: >
  The constitutional skill for the AHS Ecosystem V3.1. Governs the Cellular Architecture
  pattern: each Bounded Context is an Autonomous Cell — a self-contained Micro-SaaS unit
  deployable standalone or integrated into the AHS Control Tower. Use this skill whenever
  the user mentions AHS Blueprint, V3.1, Cellular Architecture, Autonomous Cell, Cell
  namespace, Database-per-Cell, AHS.Cell, Control Tower, Micro-SaaS factory, cellular
  isolation, Clean Architecture + DDD + CQRS in a cell, or the 5-Layer Skill Stack.
  Trigger on: AHS Blueprint, Cellular Architecture, Autonomous Cell, AHS.Cell, Control Tower,
  Micro-SaaS factory, Database-per-Cell, cellular isolation, cell scaffold, cell template.
---

# AHS Universal Constitution — Blueprint V3.1
## Cellular Architecture for Autonomous & Integrated Micro-SaaS

---

## 1. The Cellular Manifesto

AHS is a **factory of Autonomous Cells**. Each Cell is a self-contained Micro-SaaS that:
- Owns its **domain model** (DDD Aggregate Roots as `record` types)
- Owns its **database** (Database-per-Cell — strict isolation)
- Owns its **GxP Ledger** (immutable SHA256 hash chain)
- Can operate **standalone** (sold independently) OR **integrated** into the Control Tower
- Communicates with other cells **only** via Azure Service Bus events (no direct DB calls)

```
AHS Control Tower (AHS.Web.UI)
    ├── Cell: AssetManager     (AHS.Cell.AssetManager.*)
    ├── Cell: ColdChain        (AHS.Cell.ColdChain.*)
    ├── Cell: FinTracker       (AHS.Cell.FinTracker.*)
    ├── Cell: ShopifyBridge    (AHS.Cell.ShopifyBridge.*)
    └── Cell: [New Domain]     (AHS.Cell.[Name].*)
```

---

## 2. The 5-Layer Skill Stack

| Layer | Name | Technologies | Owner |
|---|---|---|---|
| **C1** | Runtime | .NET 10 / C# 14 / Native AOT / SIMD AVX-512 | C1 Architect |
| **C2** | Architecture | Clean Arch + DDD + CQRS / Cellular Isolation | C2 Engineer |
| **C3** | Infrastructure | EF Core 10 (Source Gen) / Redis HybridCache / GxP Ledger | C2 Engineer |
| **C4** | Quality | NetArchTest / xUnit / Testcontainers / Reqnroll | C2 Engineer |
| **C5** | Context | Industry Domain (SaaS / FinTech / Cold Chain / Shopify) | C1 Architect |

---

## 3. Universal Namespace Hierarchy

```
AHS.Cell.[CellName].[Layer]

Examples:
  AHS.Cell.AssetManager.Domain          ← Pure domain, zero dependencies
  AHS.Cell.AssetManager.Application     ← Use cases, CQRS handlers
  AHS.Cell.AssetManager.Infrastructure  ← EF Core, Redis, Service Bus
  AHS.Cell.AssetManager.API             ← Minimal API endpoints
  AHS.Cell.AssetManager.Tests           ← All test types
  AHS.Cell.AssetManager.Contracts       ← Public events (consumed by other cells)

AHS.Web.UI                              ← Control Tower (Sovereign Elite)
AHS.Web.Common                          ← Shared Razor Class Library
AHS.Common                              ← Cross-cutting (GxP Ledger, SIMD engines)
```

---

## 4. Cell Anatomy — Clean Architecture + DDD + CQRS

```
AHS.Cell.[Name]/
├── Domain/
│   ├── Aggregates/          ← record types with factory methods
│   ├── Events/              ← DomainEvent records (immutable)
│   ├── ValueObjects/        ← readonly record structs
│   └── Ports/               ← interfaces (IRepository, IEventStore)
├── Application/
│   ├── Commands/            ← SignedCommand → CommandHandler
│   ├── Queries/             ← IQuery<T> → QueryHandler (Dapper)
│   ├── EventHandlers/       ← consume DomainEvents → project read models
│   └── Ports/               ← IEmailService, IFileStore (not infra)
├── Infrastructure/
│   ├── Persistence/         ← EF Core 10 DbContext (Source Gen)
│   ├── Repositories/        ← implements Domain ports
│   ├── EventStore/          ← GxP Ledger append-only
│   └── Adapters/            ← external integrations (Shopify, Modbus, etc.)
├── API/
│   ├── Program.cs           ← Minimal API, Native AOT
│   ├── Endpoints/           ← MapGroup per aggregate
│   └── Contracts/           ← DTOs (JsonSerializerContext)
└── Tests/
    ├── Unit/                ← Domain logic (no infra)
    ├── Integration/         ← WebApplicationFactory + Testcontainers
    ├── Architecture/        ← NetArchTest guardrails
    └── BDD/                 ← Reqnroll .feature files
```

---

## 5. Architectural Guardrails (Non-Negotiable)

### G1 — Native AOT: No Reflection
```csharp
// ❌ FORBIDDEN in any Cell
JsonSerializer.Serialize(obj);                    // no JsonSerializerContext
Assembly.GetTypes();                              // reflection scan
Activator.CreateInstance(typeof(T));              // dynamic creation
AutoMapper with reflection profiles;              // use Mapperly instead

// ✅ REQUIRED
[JsonSerializable(typeof(AssetDto))]
public partial class AssetManagerJsonContext : JsonSerializerContext { }

JsonSerializer.Serialize(dto, AssetManagerJsonContext.Default.AssetDto);
```

### G2 — Domain Immutability
```csharp
// ❌ FORBIDDEN
public class Asset { public string Name { get; set; } }

// ✅ REQUIRED — record + factory method + validated
public record Asset
{
    public Guid   Id       { get; private init; }
    public string Name     { get; private init; }
    public Guid   TenantId { get; private init; }

    private Asset() { }

    public static Asset Create(string name, Guid tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Asset { Id = Guid.NewGuid(), Name = name, TenantId = tenantId };
    }
}
```

### G3 — Database-per-Cell
```
Cell A (AssetManager) → PostgreSQL DB: ahs_assetmanager_[tenantslug]
Cell B (ColdChain)    → PostgreSQL DB: ahs_coldchain_[tenantslug]
Cell C (FinTracker)   → PostgreSQL DB: ahs_fintracker_[tenantslug]

Cross-cell queries: NEVER direct SQL joins across cell databases.
Cross-cell data: via Service Bus events + local read model projection.
```

### G4 — GxP Integrity (Universal — not just Cold Chain)
```csharp
// Every state-changing command in EVERY cell carries:
public abstract record SignedCommand
{
    public required Guid   SignedById      { get; init; }
    public required string SignedByName    { get; init; }
    public required string ReasonForChange { get; init; }  // FDA 21 CFR Part 11
    public DateTimeOffset  SignedAt { get; init; } = DateTimeOffset.UtcNow;
    protected SignedCommand()
    {
        if (string.IsNullOrWhiteSpace(ReasonForChange))
            throw new ElectronicSignatureRequiredException(
                "ReasonForChange required per GxP Integrity guardrail.");
    }
}
```

### G5 — Sovereign Elite UI
```
- Dark Mode first (HSL-dynamic, never hardcoded hex in components)
- Glassmorphism: bg-white/10, backdrop-blur-md, border-white/20
- High Density: compact tables, virtualized grids (QuickGrid)
- Blazor components from AHS.Web.Common (no duplicates across cells)
- No "AI-generated look": Inter/Roboto + purple gradient = REJECTED
```

### G6 — Inter-Cell Communication (Service Bus Only)
```csharp
// ✅ Cell publishes its Contracts (public events)
// In AHS.Cell.ColdChain.Contracts:
public record ShipmentExcursionDetected(
    Guid   ShipmentId,
    string TenantSlug,
    double CelsiusValue,
    DateTimeOffset DetectedAt) : ICellEvent;

// ✅ Another cell subscribes
// In AHS.Cell.AssetManager.Infrastructure.Adapters:
public class ColdChainEventAdapter(IAssetStatusService service)
    : ICellEventHandler<ShipmentExcursionDetected>
{
    public Task HandleAsync(ShipmentExcursionDetected evt, CancellationToken ct)
        => service.FlagAssetAtRiskAsync(evt.ShipmentId, evt.TenantSlug, ct);
}
```

---

## 6. C4 Documentation Standard

### Level 1 — Context (C1 Architect owns)
```
[External User] → (HTTPS) → [AHS Control Tower]
[AHS Control Tower] → (Service Bus) → [Cell: AssetManager]
[AHS Control Tower] → (Service Bus) → [Cell: ColdChain]
[Cell: ColdChain] → (PostgreSQL) → [DB: coldchain_tenant]
[External: Sensor Gateway] → (HTTP) → [Cell: ColdChain API]
```

### Level 2 — Containers (C1 Architect owns)
```
AHS Ecosystem:
├── Container: AHS.Web.UI         [Blazor, Azure Container Apps]
├── Container: AHS.Cell.*.API     [Minimal API, Native AOT, Container Apps]
├── Container: PostgreSQL per Cell [Azure Flexible Server]
├── Container: Azure Service Bus   [Message broker, inter-cell]
├── Container: Redis               [HybridCache L2]
└── Container: Azure Key Vault    [GxP HMAC keys, secrets]
```

### Level 3 — Components (C2 Engineer owns)
```
AHS.Cell.AssetManager.API:
├── Component: AssetEndpoints      [Minimal API, MapGroup("/assets")]
├── Component: AssetCommandHandler [CQRS, writes to EventStore]
├── Component: AssetQueryHandler   [CQRS, reads via Dapper]
├── Component: GxPLedger           [SHA256 hash chain]
└── Component: AssetDbContext      [EF Core 10, Source Gen]
```

### Level 4 — Code (C2 Engineer + AG generate)
```csharp
// C4 L4 = actual code generated by AG following C2's Prompt Maestro
// See: ahs-cell-template skill for the scaffold
```

---

## 7. Control Tower Integration Pattern

```csharp
// AHS.Web.UI aggregates all cells via their public APIs
// Each cell exposes a Blazor RCL (Razor Class Library) module

// In AHS.Web.Common — Cell registration
public static class CellRegistry
{
    public static IServiceCollection AddCell<TCell>(
        this IServiceCollection services, string baseUrl)
        where TCell : ICellModule
    {
        services.AddHttpClient<TCell>(c => c.BaseAddress = new Uri(baseUrl));
        return services;
    }
}

// Program.cs (Control Tower)
builder.Services.AddCell<AssetManagerModule>("https://assetmanager.ahs.internal");
builder.Services.AddCell<ColdChainModule>("https://coldchain.ahs.internal");
```

---

## 8. New Cell Checklist

```
□ Namespace follows AHS.Cell.[Name].[Layer]
□ Domain has ZERO external dependencies (NetArchTest enforced)
□ All domain models are record types with factory methods
□ JsonSerializerContext registered for all public types
□ SignedCommand base for all write commands
□ GxP Ledger wired (append-only, SHA256)
□ Database-per-Cell (own PostgreSQL schema/DB)
□ Public events in AHS.Cell.[Name].Contracts project
□ Blazor RCL module registered in AHS.Web.Common
□ NetArchTest suite covers all 5 guardrails
□ Reqnroll .feature files with @GxP tags
□ Bicep module in infra/cells/[name].bicep
□ Docker-compose entry for local dev
```
