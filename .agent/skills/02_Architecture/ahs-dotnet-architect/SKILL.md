---
name: ahs-dotnet-architect
description: >
  Lead Architect skill for the AHS Ecosystem V3.1. Expert in .NET 10, C# 14, Native AOT,
  SIMD-accelerated thermal physics, Clean Architecture + DDD + CQRS Cellular pattern,
  and GxP compliance for industrial Micro-SaaS. Use this skill when designing any AHS Cell,
  validating AOT compatibility, architecting for GxP compliance (Audit Trails, Electronic
  Signatures, 21 CFR Part 11), optimizing for Zero-GC environments, or producing the
  Prompt Maestro for AG. Trigger on: AHS architecture, Cell design, .NET 10, Native AOT,
  SIMD, thermal physics, GxP, Prompt Maestro, C1 spec, C2 design, Zero-Allocation,
  Sovereign Elite, cold chain architecture, Cellular Architecture.
risk: low
source: ahs-core
date_added: '2026-03-16'
version: '3.1'
---

# AHS Lead Architect — V3.1
## .NET 10 / C# 14 / Native AOT / Cellular Architecture

---

## Use this skill when

- Designing any AHS Autonomous Cell following Blueprint V3.1.
- Defining specs for high-performance backend systems in C# 14.
- Architecting for GxP compliance (Audit Trails, Electronic Signatures, 21 CFR Part 11).
- Optimizing for Native AOT and Zero-GC environments.
- Producing the **Prompt Maestro** that C2 sends to AG for code generation.
- Validating that a design is Native AOT compatible before C2 writes the first prompt.

---

## Architectural Pattern — V3.1 Canonical

> ⚠️ **V3.1 change from previous versions**: AHS Cells use **Clean Architecture + DDD + CQRS**,
> not Vertical Slice Architecture. This is a non-negotiable Blueprint guardrail — AG generates
> cells with Domain / Application / Infrastructure / API layers. Vertical Slice is not used.

```
AHS.Cell.[Name]/
├── Domain/          ← Pure domain. Zero dependencies. record types + factory methods.
├── Application/     ← CQRS handlers. Depends only on Domain.
├── Infrastructure/  ← EF Core 10, Dapper, Service Bus, Redis. Implements Domain ports.
├── Contracts/       ← Public events (ICellEvent). Consumed by other Cells.
├── API/             ← Minimal API, Native AOT, JsonSerializerContext.
└── Tests/           ← xUnit, Testcontainers, NetArchTest, Reqnroll.
```

**Why Clean Architecture over Vertical Slice for AHS:**
Cells must be independently sellable as Micro-SaaS. Clean Architecture enforces the
domain isolation that makes each Cell a defensible commercial unit. NetArchTest enforces
the boundaries automatically — no discipline required from the developer.

---

## Capabilities

### Industrial High-Performance (unchanged from V2.0)

**Hardware Acceleration — AVX-512:**
```csharp
// AHS.Engines.HPC — thermal physics engine
// Vector512<float> processes 16 temperature readings per CPU instruction
if (Avx512F.IsSupported)
{
    var acc = Vector512<float>.Zero;
    fixed (float* p = readings)
        for (int i = 0; i < vectorized; i += Vector512<float>.Count)
            acc = Avx512F.Add(acc, Vector512.Load(p + i));
    return Vector512.Sum(acc);
}
```

**Native AOT Mastery — the three rules:**
```csharp
// Rule 1: ALL serialization via JsonSerializerContext (no reflection)
[JsonSerializable(typeof(ShipmentDto))]
public partial class CellJsonContext : JsonSerializerContext { }

// Rule 2: NO Activator.CreateInstance, Assembly.GetTypes, BindingFlags
// Rule 3: Explicit DI registration (no reflection scanning)
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
```

**Zero-Allocation Logic — hot path rules:**
```csharp
// P99 < 10ms for Oracle. These patterns are mandatory in hot paths:
// ✅ Span<T> for buffer processing
Span<double> buffer = stackalloc double[256];

// ✅ ValueTask for cached results (no heap allocation on cache hit)
public async ValueTask<OracleResult> CalculateAsync(OracleRequest req, CancellationToken ct)

// ✅ readonly record struct on the stack (ThermalDataPoint is hot path)
public readonly record struct ThermalDataPoint(double CelsiusValue, DateTimeOffset Timestamp);

// ❌ NEVER in hot paths:
readings.Where(r => r > 0).Select(r => r * 1.8).Sum()  // 3 heap allocations
$"sensor_{id}_reading_{timestamp}"                       // heap allocation
```

### GxP & Regulatory Architecture (enhanced in V3.1)

**Universal GxP — applies to ALL Cells, not just Cold Chain:**
```csharp
// Every write command in every Cell inherits SignedCommand
public abstract record SignedCommand
{
    public required string ReasonForChange { get; init; }  // FDA 21 CFR Part 11 §11.10(e)
    protected SignedCommand()
    {
        if (string.IsNullOrWhiteSpace(ReasonForChange))
            throw new ElectronicSignatureRequiredException("ReasonForChange required.");
    }
}
```

**SHA256 Hash Chain — immutable ledger per Cell:**
```csharp
// Each entry seals the previous hash → any tampering breaks the chain
var entry = new LedgerEntry { ... PreviousHash = lastHash };
var withHash = entry with { EntryHash = hasher.ComputeEntryHash(entry) };
var sealed_  = withHash with { HmacSeal = hasher.ComputeHmac(withHash) };
```

**Validation-Ready (CSV — Computer System Validation):**
- NetArchTest suite enforces all 5 Blueprint guardrails automatically.
- Reqnroll BDD with `@GxP` and `@21CFR11` tags provides traceability to requirements.
- GxP Ledger export (PDF + CSV with hash verification) per Cell.

### Sensor Integration — Adapter/Port Pattern (updated V3.1)

> ⚠️ **V3.1 change**: Sensor protocols (MQTT, gRPC, Modbus, OPC-UA, HTTP) are
> **customer-chosen, not AHS-imposed**. AHS exposes a Port — customers bring their Adapter.

```csharp
// Port (Domain layer) — AHS only knows this interface
public interface IThermalDataSource
{
    IAsyncEnumerable<ThermalDataPoint> StreamAsync(string zoneId, CancellationToken ct);
}

// Adapters (Infrastructure layer) — one per customer protocol
// MqttThermalAdapter, GrpcThermalAdapter, HttpWebhookAdapter, ModbusThermalAdapter
// All translate to ThermalDataPoint — Domain never sees the wire protocol

// Inter-Cell transport: Azure Service Bus (not direct MQTT/gRPC between Cells)
// MQTT/gRPC is Customer → AHS.Cell.ColdChain.API only
```

**Hybrid Storage (unchanged):**
- **EF Core 10** (PostgreSQL): write side, change tracking, migrations, GxP Ledger
- **Dapper**: read side, CQRS queries, zero overhead, AOT-safe
- **Redis HybridCache**: L1 IMemoryCache + L2 Redis, device registry, Oracle results

---

## Instructions for C1 (Architect Role)

- Focus on **Specifications**. Produce the domain model, C4 L1-L2, and ADRs.
- Always validate AOT compatibility: no reflection, no dynamic, no Expression.Compile().
- Integrate **SIMD (Vector512)** in any logic involving thermal calculations or telemetry.
- Design for **Sovereign Elite UI**: Dark Mode first, Glassmorphism, High Density.
- Use `AHS.Web.Common` components — never raw glass CSS classes.
- Output: structured **Prompt Maestro** (9 sections) for C2 to send to AG.

---

## Response Approach — Handover to C2 (updated V3.1)

```
1. DOMAIN SPEC:      Define aggregates, events, value objects in business language.
                     Use DDD Ubiquitous Language. No CRUD language.

2. CONSTRAINT CHECK: Verify .NET 10 + Native AOT compatibility.
                     Flag any reflection-heavy dependencies upfront.

3. REGULATORY SCOPE: Map regulations to operations.
                     Which commands require SignedCommand + GxP Ledger?

4. PERFORMANCE TARGETS: Flag P99 targets for hot paths.
                         Oracle: P99 < 10ms. Sensor ingestion: P99 < 50ms.

5. PROMPT MAESTRO:   Produce the 9-section structured prompt for AG.
                     Section 0 must include all constraints explicitly.
                     Section 8 must list files in dependency order.
                     Section 9 must have binary quality gates.
```

> **Format of C1 output to C2** (V3.1 standard):
> Structured Markdown Prompt Maestro with 9 sections — not free-form YAML.
> See `prompt-engineering-ag` skill for the complete template.

---

## Namespace Convention (mandatory)

```
AHS.Cell.[CellName].[Layer]

AHS.Cell.ColdChain.Domain
AHS.Cell.ColdChain.Application
AHS.Cell.ColdChain.Infrastructure
AHS.Cell.ColdChain.Contracts      ← public events for other Cells
AHS.Cell.ColdChain.API
AHS.Cell.ColdChain.Tests

AHS.Web.UI                        ← Control Tower (Blazor)
AHS.Web.Common                    ← Sovereign Elite RCL
AHS.Common                        ← GxP Ledger, SIMD engines (shared)
AHS.ControlTower.BFF              ← BFF for real-time + analytical aggregation
```

---

## Quick Reference — What Changed in V3.1

| V2.0 / Previous | V3.1 Current | Impact |
|---|---|---|
| Vertical Slice Architecture | Clean Architecture + DDD + CQRS | Cell folder structure changes |
| MQTT hardcoded for sensors | Adapter/Port — protocol agnostic | Sensor code moves to Infrastructure |
| C1 outputs YAML spec | C1 outputs Prompt Maestro (9 sections) | C2 has more explicit instructions |
| Cold Chain only | Universal Cells (any domain) | GxP applies to ALL Cells |
| SQL Server | PostgreSQL 17 (Npgsql) | SQL dialect, DDL types |
| Free-form architecture | `AHS.Cell.[Name].[Layer]` namespace | Consistent across all Cells |
