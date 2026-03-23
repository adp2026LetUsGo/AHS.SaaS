---
name: gxp-ledger-eventsourcing
description: >
  Expert guidance on GxP-compliant immutable audit ledgers and event sourcing in C# for
  AHS (cold chain industrial). Use this skill whenever the user mentions GxP ledger,
  immutable audit trail, event sourcing, append-only store, event replay, snapshots,
  projections, FDA 21 CFR Part 11, EU Annex 11, ALCOA+, audit log, domain events,
  IEventStore, EventStream, AggregateRoot, event versioning, or tamper-evident logs.
  Trigger on: GxP, 21 CFR Part 11, Annex 11, ALCOA, event sourcing, append-only,
  immutable log, audit trail, domain event, event store, aggregate root, snapshot,
  projection, event replay, ledger entry.
---

# GxP Ledger & Event Sourcing — C# 14 / .NET 10 / Native AOT

## Regulatory Context (AHS)

| Regulation | Requirement | Implementation |
|---|---|---|
| FDA 21 CFR Part 11 | Audit trail, e-signatures, tamper detection | SHA256 hash chain + HMAC |
| EU GMP Annex 11 | Audit trail with timestamps, user identity | `[PersistentState]` + claims |
| ALCOA+ | Attributable, Legible, Contemporaneous, Original, Accurate | Append-only store + UTC timestamps |
| ICH Q10 | Change control, deviation management | Event versioning + projections |

---

## 0. Capa 5 — Electronic Signatures & Reason for Change (FDA 21 CFR Part 11)

> Cada comando de escritura en AHS **debe** llevar firma electrónica y motivo.
> Aplica a: `SealShipmentCommand`, `ApplyWhatIfChangeCommand`, `ResolveExcursionCommand`,
> y cualquier otro comando que mute estado en el Ledger GxP.

```csharp
// Base de todos los comandos de escritura — no puede instanciarse sin ReasonForChange
public abstract record SignedCommand
{
    public required Guid   SignedById      { get; init; }
    public required string SignedByName    { get; init; }
    public required string ReasonForChange { get; init; }  // FDA 21 CFR Part 11 §11.10(e)
    public DateTimeOffset  SignedAt        { get; init; } = DateTimeOffset.UtcNow;

    // Validación en constructor del record — C# 14 primary ctor compatible
    protected SignedCommand()
    {
        if (string.IsNullOrWhiteSpace(ReasonForChange))
            throw new ElectronicSignatureRequiredException(
                "ReasonForChange is required for all write operations per FDA 21 CFR Part 11.");
    }
}

public record SealShipmentCommand : SignedCommand
{
    public required Guid   ShipmentId      { get; init; }
    public required string FinalStatus     { get; init; }
    public required string QualityDecision { get; init; }
}

// What-If Simulator: cada cambio de parámetro queda sellado en el Ledger
public record ApplyWhatIfChangeCommand : SignedCommand
{
    public required Guid   ShipmentId     { get; init; }
    public required string ParameterName  { get; init; }  // "RouteCategory", "InsulationType", etc.
    public required string PreviousValue  { get; init; }
    public required string NewValue       { get; init; }
}

public class ElectronicSignatureRequiredException(string message) : Exception(message);
```

```csharp
// Handler — aplica el comando y sella en el Ledger con la firma electrónica
public class WhatIfChangeHandler(IEventStore store, LedgerHasher hasher, ITenantContext tenant)
{
    public async Task HandleAsync(ApplyWhatIfChangeCommand cmd, CancellationToken ct)
    {
        // El ReasonForChange ya fue validado en el constructor del record
        var evt = new WhatIfParameterChanged(
            ShipmentId:    cmd.ShipmentId,
            ParameterName: cmd.ParameterName,
            PreviousValue: cmd.PreviousValue,
            NewValue:      cmd.NewValue,
            // Firma electrónica sellada dentro del evento — trazabilidad completa
            SignedById:    cmd.SignedById.ToString(),
            SignedByName:  cmd.SignedByName,
            ReasonForChange: cmd.ReasonForChange,  // ← va al Ledger inmutable con SHA256
            SignedAt:      cmd.SignedAt)
        {
            TenantId  = tenant.TenantId,
            ActorId   = cmd.SignedById,
            ActorName = cmd.SignedByName,
            EventType = nameof(WhatIfParameterChanged),
        };

        await store.AppendAsync(cmd.ShipmentId, "WhatIfSession", [evt],
            expectedVersion: 0, ct);
    }
}

// Endpoint Blazor — exige firma antes de aceptar el cambio
public record WhatIfParameterChanged(
    Guid   ShipmentId,
    string ParameterName,
    string PreviousValue,
    string NewValue,
    string SignedById,
    string SignedByName,
    string ReasonForChange,  // sellado con SHA256 en el Ledger — inmutable
    DateTimeOffset SignedAt
) : DomainEvent;
```

---

## 0. Paquetes NuGet (AHS Stack)

```xml
<!-- .csproj -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.*" />
<PackageReference Include="Dapper"  Version="2.*" />
<PackageReference Include="Npgsql"  Version="9.*" />
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.*" />
```

> **EF Core 10** → escrituras transaccionales del ledger (concurrencia optimista, interceptores RLS).
> **Dapper** → lecturas de auditoría, verificación de cadena, projections (zero overhead, AOT-safe).
> **Service Bus** → publicación de `DomainEvent` para subscribers externos (alertas, Oracle, UI).

---

## 0b. AOT Setup

```csharp
[JsonSerializable(typeof(LedgerEvent))]
[JsonSerializable(typeof(ShipmentCreated))]
[JsonSerializable(typeof(TemperatureExcursionRecorded))]
[JsonSerializable(typeof(ExcursionResolved))]
[JsonSerializable(typeof(CarrierAssigned))]
[JsonSerializable(typeof(ShipmentSealed))]
[JsonSerializable(typeof(LedgerSnapshot))]
[JsonSerializable(typeof(List<LedgerEvent>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class LedgerJsonContext : JsonSerializerContext { }
```

---

## 1. Core Domain Events

```csharp
// Base — ALL events are immutable records (Blueprint guardrail: record types)
public abstract record DomainEvent
{
    public Guid        EventId     { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public string      EventType   { get; init; } = "";
    public Guid        TenantId    { get; init; }
    public Guid        ActorId     { get; init; }   // who triggered it (GxP: attributable)
    public string      ActorName   { get; init; } = ""; // ALCOA: Attributable
    public int         Version     { get; init; } = 1;
}

// AHS-specific events
public record ShipmentCreated(
    Guid   ShipmentId,
    string CargoType,         // Pharma | Food | Chemical
    string OriginLocation,
    string DestinationLocation,
    DateTimeOffset PlannedDeparture,
    string InsulationType     // Active | Passive — Passive triggers +15% Oracle penalty
) : DomainEvent
{
    public string EventType { get; init; } = nameof(ShipmentCreated);
}

public record TemperatureExcursionRecorded(
    Guid   ShipmentId,
    Guid   SensorId,
    string ZoneId,
    double ObservedCelsius,
    double MinLimit,
    double MaxLimit,
    DateTimeOffset ExcursionStart,
    ExcursionSeverity Severity
) : DomainEvent
{
    public string EventType { get; init; } = nameof(TemperatureExcursionRecorded);
}

public record ExcursionResolved(
    Guid   ShipmentId,
    Guid   ExcursionEventId,  // links back to TemperatureExcursionRecorded
    DateTimeOffset ResolvedAt,
    string ResolutionNote,
    string ResponsibleOperator
) : DomainEvent
{
    public string EventType { get; init; } = nameof(ExcursionResolved);
}

public record ShipmentSealed(
    Guid   ShipmentId,
    string FinalStatus,       // Compliant | NonCompliant | UnderReview
    string MktCelsius,        // Mean Kinetic Temp at closure
    string QualityDecision,   // Accept | Reject | Quarantine
    string QualityOfficerId
) : DomainEvent
{
    public string EventType { get; init; } = nameof(ShipmentSealed);
}
```

---

## 2. Immutable Ledger Entry (GxP Hash Chain)

```csharp
// Each entry seals itself + the previous hash — tamper-evident chain
public record LedgerEntry
{
    public long            Sequence      { get; init; }
    public Guid            TenantId      { get; init; }
    public Guid            AggregateId   { get; init; }
    public string          AggregateType { get; init; } = "";
    public string          EventType     { get; init; } = "";
    public string          PayloadJson   { get; init; } = "";  // AOT serialized
    public string          ActorId       { get; init; } = "";
    public string          ActorName     { get; init; } = "";
    public DateTimeOffset  OccurredAt    { get; init; }
    public string          PreviousHash  { get; init; } = "GENESIS";
    public string          EntryHash     { get; init; } = "";  // computed on write
    public string          HmacSeal      { get; init; } = "";  // HMAC-SHA256
}

public class LedgerHasher(byte[] hmacKey)
{
    // Deterministic canonical string — order matters for reproducibility
    private static string Canonical(LedgerEntry e)
        => $"{e.Sequence}|{e.TenantId}|{e.AggregateId}|{e.EventType}|{e.PayloadJson}|{e.OccurredAt:O}|{e.PreviousHash}";

    public string ComputeEntryHash(LedgerEntry e)
    {
        var raw = Encoding.UTF8.GetBytes(Canonical(e));
        return Convert.ToHexString(SHA256.HashData(raw));
    }

    public string ComputeHmac(LedgerEntry e)
    {
        Span<byte> mac = stackalloc byte[HMACSHA256.HashSizeInBytes];
        HMACSHA256.HashData(hmacKey, Encoding.UTF8.GetBytes(e.EntryHash), mac);
        return Convert.ToBase64String(mac);
    }

    public bool VerifyEntry(LedgerEntry e)
    {
        var expectedHash = ComputeEntryHash(e);
        if (expectedHash != e.EntryHash) return false;

        Span<byte> actualMac  = stackalloc byte[HMACSHA256.HashSizeInBytes];
        Span<byte> expectedMac = stackalloc byte[HMACSHA256.HashSizeInBytes];

        HMACSHA256.HashData(hmacKey, Encoding.UTF8.GetBytes(e.EntryHash), actualMac);
        Convert.TryFromBase64String(e.HmacSeal, expectedMac, out _);

        return CryptographicOperations.FixedTimeEquals(actualMac, expectedMac);
    }

    public bool VerifyChain(IReadOnlyList<LedgerEntry> entries)
    {
        string expectedPrev = "GENESIS";
        foreach (var entry in entries)
        {
            if (entry.PreviousHash != expectedPrev) return false;
            if (!VerifyEntry(entry)) return false;
            expectedPrev = entry.EntryHash;
        }
        return true;
    }
}
```

---

## 3. Aggregate Root Pattern

```csharp
// Blueprint guardrail: zero domain dependencies, factory methods
public abstract class AggregateRoot
{
    public Guid   Id           { get; protected set; }
    public Guid   TenantId     { get; protected set; }
    public int    Version      { get; private set; } = 0;

    private readonly List<DomainEvent> _uncommitted = [];
    public  IReadOnlyList<DomainEvent> UncommittedEvents => _uncommitted;

    protected void Apply(DomainEvent evt)
    {
        When(evt);
        _uncommitted.Add(evt);
        Version++;
    }

    // Replay from store — rebuilds state without side effects
    public void Rehydrate(IEnumerable<DomainEvent> history)
    {
        foreach (var evt in history)
        {
            When(evt);
            Version++;
        }
    }

    protected abstract void When(DomainEvent evt);
    public void ClearUncommitted() => _uncommitted.Clear();
}

// AHS Shipment Aggregate
public class Shipment : AggregateRoot
{
    public string          Status        { get; private set; } = "Draft";
    public string          CargoType     { get; private set; } = "";
    public string          InsulationType { get; private set; } = "";
    public List<Guid>      Excursions    { get; private set; } = [];
    public bool            IsSealed      { get; private set; }

    private Shipment() { }  // for rehydration

    // ✅ Factory method (Blueprint: validated factory methods)
    public static Shipment Create(Guid tenantId, Guid actorId, string actorName,
        string cargoType, string origin, string destination,
        DateTimeOffset plannedDeparture, string insulationType)
    {
        var shipment = new Shipment();
        shipment.Apply(new ShipmentCreated(
            ShipmentId:      Guid.NewGuid(),
            CargoType:       cargoType,
            OriginLocation:  origin,
            DestinationLocation: destination,
            PlannedDeparture: plannedDeparture,
            InsulationType:  insulationType)
        {
            TenantId  = tenantId,
            ActorId   = actorId,
            ActorName = actorName,
        });
        return shipment;
    }

    public void RecordExcursion(Guid sensorId, string zoneId, double celsius,
        double min, double max, ExcursionSeverity severity, Guid actorId, string actorName)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot record excursion on sealed shipment.");
        Apply(new TemperatureExcursionRecorded(Id, sensorId, zoneId, celsius, min, max,
            DateTimeOffset.UtcNow, severity)
        {
            TenantId = TenantId, ActorId = actorId, ActorName = actorName,
        });
    }

    public void Seal(string finalStatus, double mkt, string qualityDecision,
        string qualityOfficerId, string officerName)
    {
        if (IsSealed) throw new InvalidOperationException("Shipment already sealed.");
        Apply(new ShipmentSealed(Id, finalStatus, mkt.ToString("F2"),
            qualityDecision, qualityOfficerId)
        {
            TenantId = TenantId, ActorId = Guid.Parse(qualityOfficerId), ActorName = officerName,
        });
    }

    protected override void When(DomainEvent evt)
    {
        switch (evt)
        {
            case ShipmentCreated e:
                Id             = e.ShipmentId;
                TenantId       = e.TenantId;
                CargoType      = e.CargoType;
                InsulationType = e.InsulationType;
                Status         = "Active";
                break;

            case TemperatureExcursionRecorded e:
                Excursions.Add(e.EventId);
                if (e.Severity == ExcursionSeverity.Critical)
                    Status = "UnderReview";
                break;

            case ExcursionResolved:
                if (Status == "UnderReview" && Excursions.All(_ => true))
                    Status = "Active";
                break;

            case ShipmentSealed e:
                IsSealed = true;
                Status   = e.FinalStatus;
                break;
        }
    }
}
```

---

## 4. Event Store (Append-Only, PostgreSQL + Dapper)

> **EF Core** para escrituras transaccionales (concurrencia optimista, interceptores).
> **Dapper** para lecturas de auditoría y verificación de cadena (queries directas, zero overhead).

```csharp
public interface IEventStore
{
    Task AppendAsync(Guid aggregateId, string aggregateType,
        IReadOnlyList<DomainEvent> events, int expectedVersion, CancellationToken ct);
    Task<IReadOnlyList<DomainEvent>> LoadAsync(Guid aggregateId, CancellationToken ct);
    Task<IReadOnlyList<DomainEvent>> LoadFromAsync(Guid aggregateId, int fromVersion, CancellationToken ct);
}

public class SqlEventStore(
    IDbConnectionFactory db,
    LedgerHasher hasher,
    ITenantContext tenant,
    ILogger<SqlEventStore> logger) : IEventStore
{
    public async Task AppendAsync(Guid aggregateId, string aggregateType,
        IReadOnlyList<DomainEvent> events, int expectedVersion, CancellationToken ct)
    {
        await using var conn = await db.CreateAsync(ct);
        await using var tx   = await conn.BeginTransactionAsync(ct);

        // Optimistic concurrency check
        var currentVersion = await conn.ExecuteScalarAsync<int>(
            "SELECT COALESCE(MAX(version), 0) FROM ledger_entries WHERE aggregate_id = @id AND tenant_id = @tid",
            new { id = aggregateId, tid = tenant.TenantId }, tx);

        if (currentVersion != expectedVersion)
            throw new ConcurrencyException(aggregateId, expectedVersion, currentVersion);

        var lastHash = await conn.ExecuteScalarAsync<string>(
            "SELECT entry_hash FROM ledger_entries WHERE aggregate_id = @id AND tenant_id = @tid ORDER BY sequence DESC LIMIT 1",
            new { id = aggregateId, tid = tenant.TenantId }, tx) ?? "GENESIS";

        var sequence = await conn.ExecuteScalarAsync<long>(
            "SELECT COALESCE(MAX(sequence), 0) FROM ledger_entries WHERE tenant_id = @tid",
            new { tid = tenant.TenantId }, tx);

        foreach (var evt in events)
        {
            sequence++;
            var payload = SerializeEvent(evt);

            var entry = new LedgerEntry
            {
                Sequence      = sequence,
                TenantId      = tenant.TenantId,
                AggregateId   = aggregateId,
                AggregateType = aggregateType,
                EventType     = evt.EventType,
                PayloadJson   = payload,
                ActorId       = evt.ActorId.ToString(),
                ActorName     = evt.ActorName,
                OccurredAt    = evt.OccurredAt,
                PreviousHash  = lastHash,
            };

            // Compute hash then HMAC — order matters
            var withHash = entry with { EntryHash = hasher.ComputeEntryHash(entry) };
            var sealed_  = withHash with { HmacSeal = hasher.ComputeHmac(withHash) };

            await conn.ExecuteAsync("""
                INSERT INTO ledger_entries
                  (sequence,tenant_id,aggregate_id,aggregate_type,event_type,
                   payload_json,actor_id,actor_name,occurred_at,previous_hash,entry_hash,hmac_seal)
                VALUES
                  (@Sequence,@TenantId,@AggregateId,@AggregateType,@EventType::jsonb,
                   @PayloadJson,@ActorId,@ActorName,@OccurredAt,@PreviousHash,@EntryHash,@HmacSeal)
                """, sealed_, tx);

            lastHash = sealed_.EntryHash;
        }

        await tx.CommitAsync(ct);
        logger.LogInformation("Appended {Count} events to {AggregateId}", events.Count, aggregateId);
    }

    public async Task<IReadOnlyList<DomainEvent>> LoadAsync(Guid aggregateId, CancellationToken ct)
    {
        await using var conn = await db.CreateAsync(ct);
        var entries = await conn.QueryAsync<LedgerEntry>(
            "SELECT * FROM ledger_entries WHERE aggregate_id = @id AND tenant_id = @tid ORDER BY sequence",
            new { id = aggregateId, tid = tenant.TenantId });

        return entries.Select(DeserializeEvent).ToList();
    }

    // AOT-safe dispatch table — no reflection
    private static string SerializeEvent(DomainEvent evt) => evt switch
    {
        ShipmentCreated e              => JsonSerializer.Serialize(e, LedgerJsonContext.Default.ShipmentCreated),
        TemperatureExcursionRecorded e => JsonSerializer.Serialize(e, LedgerJsonContext.Default.TemperatureExcursionRecorded),
        ExcursionResolved e            => JsonSerializer.Serialize(e, LedgerJsonContext.Default.ExcursionResolved),
        ShipmentSealed e               => JsonSerializer.Serialize(e, LedgerJsonContext.Default.ShipmentSealed),
        _                              => throw new NotSupportedException($"Unknown event: {evt.EventType}")
    };

    private static DomainEvent DeserializeEvent(LedgerEntry entry) => entry.EventType switch
    {
        nameof(ShipmentCreated)              => JsonSerializer.Deserialize(entry.PayloadJson, LedgerJsonContext.Default.ShipmentCreated)!,
        nameof(TemperatureExcursionRecorded) => JsonSerializer.Deserialize(entry.PayloadJson, LedgerJsonContext.Default.TemperatureExcursionRecorded)!,
        nameof(ExcursionResolved)            => JsonSerializer.Deserialize(entry.PayloadJson, LedgerJsonContext.Default.ExcursionResolved)!,
        nameof(ShipmentSealed)               => JsonSerializer.Deserialize(entry.PayloadJson, LedgerJsonContext.Default.ShipmentSealed)!,
        _                                    => throw new NotSupportedException($"Unknown event type: {entry.EventType}")
    };
}
```

---

## 5. Snapshots (Performance at Scale)

```csharp
public record LedgerSnapshot(
    Guid   AggregateId,
    Guid   TenantId,
    int    Version,
    string AggregateType,
    string StateJson,         // AOT serialized aggregate state
    DateTimeOffset CreatedAt,
    string StateHash);        // SHA256 of StateJson

public class SnapshotStore(IDbConnectionFactory db, ITenantContext tenant)
{
    private const int SnapshotThreshold = 50; // snapshot every 50 events

    public async Task<(LedgerSnapshot? Snapshot, IReadOnlyList<DomainEvent> Events)>
        LoadWithSnapshotAsync(Guid aggregateId, IEventStore store, CancellationToken ct)
    {
        var snapshot = await LoadLatestAsync(aggregateId, ct);
        var events   = snapshot is null
            ? await store.LoadAsync(aggregateId, ct)
            : await store.LoadFromAsync(aggregateId, snapshot.Version, ct);

        return (snapshot, events);
    }

    public async Task SaveIfNeededAsync(AggregateRoot aggregate, string stateJson, CancellationToken ct)
    {
        if (aggregate.Version % SnapshotThreshold != 0) return;

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(stateJson)));
        var snapshot = new LedgerSnapshot(
            aggregate.Id, aggregate.TenantId, aggregate.Version,
            aggregate.GetType().Name, stateJson, DateTimeOffset.UtcNow, hash);

        await using var conn = await db.CreateAsync(ct);
        await conn.ExecuteAsync(
            "INSERT INTO ledger_snapshots (aggregate_id,tenant_id,version,aggregate_type,state_json,created_at,state_hash) VALUES (@AggregateId,@TenantId,@Version,@AggregateType,@StateJson::jsonb,@CreatedAt,@StateHash)",
            snapshot);
    }

    private async Task<LedgerSnapshot?> LoadLatestAsync(Guid aggregateId, CancellationToken ct)
    {
        await using var conn = await db.CreateAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<LedgerSnapshot>(
            "SELECT * FROM ledger_snapshots WHERE aggregate_id = @id AND tenant_id = @tid ORDER BY version DESC LIMIT 1",
            new { id = aggregateId, tid = tenant.TenantId });
    }
}
```

---

## 6. Read Model Projection (CQRS)

```csharp
// Blueprint: Clean Boundaries — projections live in Application layer
public class ShipmentSummaryProjection(IShipmentSummaryRepository repo)
{
    public async Task HandleAsync(DomainEvent evt, CancellationToken ct)
    {
        switch (evt)
        {
            case ShipmentCreated e:
                await repo.UpsertAsync(new ShipmentSummary(
                    e.ShipmentId, e.TenantId, "Active", e.CargoType,
                    e.OriginLocation, e.DestinationLocation,
                    ExcursionCount: 0, IsCompliant: true), ct);
                break;

            case TemperatureExcursionRecorded e:
                await repo.IncrementExcursionsAsync(e.ShipmentId, e.TenantId,
                    e.Severity == ExcursionSeverity.Critical, ct);
                break;

            case ShipmentSealed e:
                await repo.SetFinalStatusAsync(e.ShipmentId, e.TenantId,
                    e.FinalStatus, e.QualityDecision, ct);
                break;
        }
    }
}
```

---

## 7. EF Core Migration — Append-Only Table (PostgreSQL)

```sql
-- PostgreSQL DDL — tipos nativos, sin NVARCHAR ni UNIQUEIDENTIFIER
CREATE TABLE ledger_entries (
    sequence       BIGSERIAL        NOT NULL,          -- equivalente a IDENTITY(1,1)
    tenant_id      UUID             NOT NULL,          -- UUID nativo en PostgreSQL
    aggregate_id   UUID             NOT NULL,
    aggregate_type VARCHAR(100)     NOT NULL,
    event_type     VARCHAR(200)     NOT NULL,
    payload_json   JSONB            NOT NULL,          -- JSONB: indexable, validado
    actor_id       VARCHAR(100)     NOT NULL,
    actor_name     VARCHAR(200)     NOT NULL,
    occurred_at    TIMESTAMPTZ      NOT NULL,          -- con timezone — GxP: siempre UTC
    previous_hash  CHAR(64)         NOT NULL,
    entry_hash     CHAR(64)         NOT NULL,
    hmac_seal      VARCHAR(88)      NOT NULL,
    CONSTRAINT pk_ledger_entries PRIMARY KEY (tenant_id, sequence),
    CONSTRAINT uq_ledger_entry_hash UNIQUE (tenant_id, entry_hash)
);

-- RLS — append-only por tenant a nivel de base de datos
ALTER TABLE ledger_entries ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation ON ledger_entries
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);

-- Deny mutation at DB level (GxP: inmutabilidad garantizada incluso con acceso directo a BD)
REVOKE UPDATE, DELETE ON ledger_entries FROM app_role;

-- Índices para audit queries y replay
CREATE INDEX ix_ledger_aggregate ON ledger_entries (tenant_id, aggregate_id, sequence);
CREATE INDEX ix_ledger_occurred  ON ledger_entries (tenant_id, occurred_at DESC);

-- Tabla de snapshots
CREATE TABLE ledger_snapshots (
    aggregate_id   UUID         NOT NULL,
    tenant_id      UUID         NOT NULL,
    version        INTEGER      NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    state_json     JSONB        NOT NULL,
    created_at     TIMESTAMPTZ  NOT NULL,
    state_hash     CHAR(64)     NOT NULL,
    CONSTRAINT pk_ledger_snapshots PRIMARY KEY (tenant_id, aggregate_id, version)
);
```

---

## 8. GxP Compliance Checklist

| ALCOA+ Principle | Implementation |
|---|---|
| **Attributable** | `ActorId` + `ActorName` on every event |
| **Legible** | JSON payload + human-readable `EventType` |
| **Contemporaneous** | `OccurredAt = DateTimeOffset.UtcNow` at event creation |
| **Original** | Append-only SQL + DENY UPDATE/DELETE |
| **Accurate** | SHA256 hash chain + HMAC seal |
| **Complete** | `VerifyChain()` on audit export |
| **Consistent** | UTC timestamps throughout |
| **Enduring** | Snapshots + cold storage export |
| **Available** | Read model projections for fast queries |
