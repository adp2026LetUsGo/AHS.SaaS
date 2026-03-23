---
name: cqrs-clean-architecture-patterns
description: >
  Expert guidance on CQRS and Clean Architecture implementation patterns for C2 Lead
  Engineer in AHS. Covers command/query separation, when to use EF Core vs Dapper,
  read model projections, event handler patterns, command handler structure, AOT-safe
  mediator patterns (no MediatR reflection), domain service patterns, and Clean
  Architecture layer enforcement. Use this skill whenever the user mentions CQRS,
  command handler, query handler, read model, projection, when to use Dapper vs EF Core,
  mediator without MediatR, domain service, application service, clean architecture layers,
  or asks how to structure a handler in AHS.
  Trigger on: CQRS, command handler, query handler, read model, projection, Dapper vs EF Core,
  mediator, dispatcher, domain service, application service, handler pattern,
  clean architecture, layer dependency, use case.
---

# CQRS & Clean Architecture Patterns — C2 Reference
## AHS Ecosystem / Blueprint V3.1 / C# 14 / .NET 10 / Native AOT

---

## 1. The Decision Matrix: EF Core vs Dapper

The single most important C2 decision per query. Use this table:

| Scenario | Use | Reason |
|---|---|---|
| Write side (commands) | **EF Core 10** | Change tracking, optimistic concurrency, interceptors (tenant RLS) |
| Read side (queries, DTOs) | **Dapper** | Zero overhead, direct SQL, no change tracking, AOT-safe |
| Complex aggregations for reports | **Dapper** | Raw SQL is clearer and faster for multi-join reports |
| Bulk inserts (sensor data) | **Dapper + COPY** | PostgreSQL COPY is 10x faster than EF Core bulk insert |
| Migration definitions | **EF Core** | Migrations own the schema history |
| GxP Ledger reads (audit export) | **Dapper** | Ledger is append-only — no change tracking needed |
| Event replay (rehydration) | **Dapper** | Load ordered list of events — pure read, no tracking |

```csharp
// ✅ Command: EF Core (needs change tracking + interceptors)
public class RegisterAssetHandler(AssetDbContext db, IEventStore store)
{
    public async Task<Guid> HandleAsync(RegisterAssetCommand cmd, CancellationToken ct)
    {
        var asset = Asset.Create(cmd.Name, cmd.Category, cmd.TenantId, cmd.SignedById, cmd.SignedByName, cmd.ReasonForChange);
        await store.AppendAsync(asset.Id, "Asset", asset.UncommittedEvents, 0, ct);
        asset.ClearUncommitted();
        return asset.Id;
    }
}

// ✅ Query: Dapper (no EF Core, no change tracking, no overhead)
public class GetAssetByIdHandler(IDbConnectionFactory db)
{
    public async Task<AssetDto?> HandleAsync(GetAssetByIdQuery qry, CancellationToken ct)
    {
        await using var conn = await db.CreateAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<AssetDto>(
            "SELECT id, name, category, status, next_maintenance_at FROM assets WHERE id = @id",
            new { id = qry.AssetId });
    }
}
```

---

## 2. AOT-Safe Dispatcher (No MediatR)

MediatR uses reflection — incompatible with Native AOT. Use explicit dispatch:

```csharp
// Option A — Direct injection (simplest, preferred for small cells)
// Inject the specific handler you need. No dispatcher needed.
app.MapPost("/api/assets", async (
    CreateAssetRequest req,
    RegisterAssetHandler handler,  // ← direct injection
    ITenantContext tenant,
    ClaimsPrincipal user,
    CancellationToken ct) =>
{
    var cmd = new RegisterAssetCommand(
        Name:           req.Name,
        Category:       req.Category,
        TenantId:       tenant.TenantId,
        SignedById:     user.GetUserId(),
        SignedByName:   user.GetDisplayName(),
        ReasonForChange: req.ReasonForChange);

    var id = await handler.HandleAsync(cmd, ct);
    return Results.Created($"/api/assets/{id}", new { id });
});

// Option B — Typed dispatcher (when you want handler isolation from endpoints)
// AOT-safe: explicit registration, no reflection
public class CellDispatcher(IServiceProvider sp)
{
    // Compile-time dispatch table — switch expression, no reflection
    public Task<TResult> SendAsync<TResult>(object command, CancellationToken ct)
        => command switch
        {
            RegisterAssetCommand cmd => sp.GetRequiredService<RegisterAssetHandler>()
                                          .HandleAsync(cmd, ct) as Task<TResult>,
            ScheduleMaintenanceCommand cmd => sp.GetRequiredService<ScheduleMaintenanceHandler>()
                                               .HandleAsync(cmd, ct) as Task<TResult>,
            _ => throw new NotSupportedException($"No handler for {command.GetType().Name}")
        };
}
```

---

## 3. Command Handler Pattern (standard for all AHS Cells)

```csharp
// Standard command handler structure — copy for every new handler
public sealed class [Name]Handler(
    IEventStore store,          // append domain events to GxP Ledger
    I[Name]Repository repo,     // optional: only if you need to load aggregate
    ICellEventPublisher pub,    // publish to Service Bus after commit
    ILogger<[Name]Handler> log)
{
    public async Task<[ResultType]> HandleAsync([Name]Command cmd, CancellationToken ct)
    {
        // 1. Load aggregate (if mutating existing)
        var aggregate = await repo.LoadAsync(cmd.AggregateId, ct);

        // 2. Execute domain logic (no infrastructure here)
        aggregate.[DomainMethod](cmd.Param1, cmd.Param2, cmd.SignedById, cmd.SignedByName, cmd.ReasonForChange);

        // 3. Persist events (atomic — one transaction)
        await store.AppendAsync(
            aggregate.Id,
            nameof([AggregateType]),
            aggregate.UncommittedEvents,
            aggregate.Version - aggregate.UncommittedEvents.Count,  // expected version
            ct);

        aggregate.ClearUncommitted();

        // 4. Publish cell event (after DB commit — use Outbox if exactly-once needed)
        foreach (var evt in aggregate.UncommittedEvents)
            await pub.PublishAsync(evt, ct);

        log.LogInformation("[Name]Handler completed for {AggregateId}", aggregate.Id);
        return [result];
    }
}
```

---

## 4. Query Handler Pattern (Dapper, read-side)

```csharp
// Standard query handler — no EF Core, no aggregate loading
public sealed class List[Name]ByTenantHandler(IDbConnectionFactory db)
{
    public async Task<IReadOnlyList<[Name]SummaryDto>> HandleAsync(
        List[Name]ByTenantQuery qry, CancellationToken ct)
    {
        await using var conn = await db.CreateAsync(ct);  // sets tenant RLS via set_config

        // Cursor-based pagination — better than OFFSET for large datasets
        var sql = qry.AfterCursor is null
            ? """
              SELECT id, name, status, created_at
              FROM [tablename]s
              ORDER BY created_at DESC, id
              LIMIT @pageSize
              """
            : """
              SELECT id, name, status, created_at
              FROM [tablename]s
              WHERE (created_at, id) < (@cursor_time, @cursor_id)
              ORDER BY created_at DESC, id
              LIMIT @pageSize
              """;

        var results = await conn.QueryAsync<[Name]SummaryDto>(sql, new
        {
            pageSize   = qry.PageSize,
            cursor_time = qry.AfterCursor?.CreatedAt,
            cursor_id   = qry.AfterCursor?.Id,
        });

        return results.AsList();
    }
}
```

---

## 5. Read Model Projections (eventual consistency)

```csharp
// Projections are event handlers that maintain read models
// They run asynchronously — separate from the command pipeline

// Service Bus consumer → projection
public class AssetSummaryProjection(IDbConnectionFactory db)
    : ICellEventHandler<AssetRegistered>,
      ICellEventHandler<AssetRetired>
{
    public async Task HandleAsync(AssetRegistered evt, CancellationToken ct)
    {
        await using var conn = await db.CreateAsync(ct);
        // Upsert read model (idempotent — Service Bus can redeliver)
        await conn.ExecuteAsync("""
            INSERT INTO asset_summaries (id, tenant_id, name, category, status, registered_at)
            VALUES (@Id, @TenantId, @Name, @Category, 'Active', @OccurredAt)
            ON CONFLICT (id) DO UPDATE
              SET name = EXCLUDED.name,
                  status = EXCLUDED.status
            """, new { evt.AssetId, evt.TenantId, evt.Name, evt.Category, evt.OccurredAt });
    }

    public async Task HandleAsync(AssetRetired evt, CancellationToken ct)
    {
        await using var conn = await db.CreateAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE asset_summaries SET status = 'Retired', retired_at = @RetiredAt WHERE id = @Id",
            new { evt.AssetId, evt.RetiredAt });
    }
}

// Registration (AOT-safe dispatch table, not reflection scan)
builder.Services.AddScoped<AssetSummaryProjection>();
builder.Services.AddScoped<ICellEventHandler<AssetRegistered>>(sp =>
    sp.GetRequiredService<AssetSummaryProjection>());
builder.Services.AddScoped<ICellEventHandler<AssetRetired>>(sp =>
    sp.GetRequiredService<AssetSummaryProjection>());
```

---

## 6. Domain Service vs Application Service (when to use which)

```
Domain Service:
  - Lives in Domain layer (zero infrastructure dependencies)
  - Contains business logic that doesn't naturally belong to one aggregate
  - Is stateless — no DI, no async, pure calculation
  - Examples: MeanKineticTemperature, RiskScoreCalculator, TTFEngine

Application Service (= Command/Query Handler):
  - Lives in Application layer
  - Orchestrates: load aggregate → call domain → persist → publish
  - Has infrastructure dependencies (IEventStore, IRepository)
  - Is the only layer that touches infrastructure directly

Never:
  - Put infrastructure calls in Domain Services
  - Put business logic in Application Services (delegate to domain)
```

```csharp
// ✅ Domain Service — pure, stateless, no DI
public static class MeanKineticTemperature
{
    public static double Calculate(ReadOnlySpan<double> readings, double ea = 83144.0)
    {
        Span<double> exps = readings.Length <= 256 ? stackalloc double[readings.Length] : new double[readings.Length];
        for (int i = 0; i < readings.Length; i++)
            exps[i] = Math.Exp(-ea / (8.314 * (readings[i] + 273.15)));
        double avg = 0; foreach (var e in exps) avg += e; avg /= readings.Length;
        return -ea / (8.314 * Math.Log(avg)) - 273.15;
    }
}

// ✅ Application Service — orchestrates, delegates business logic to domain
public class SealShipmentHandler(IEventStore store, IShipmentRepository repo, ICellEventPublisher pub)
{
    public async Task HandleAsync(SealShipmentCommand cmd, CancellationToken ct)
    {
        var shipment = await repo.LoadAsync(cmd.ShipmentId, ct);
        var readings  = await repo.GetTemperatureReadingsAsync(cmd.ShipmentId, ct);

        // ← Domain service called from Application layer
        var mkt = MeanKineticTemperature.Calculate(
            readings.Select(r => r.CelsiusValue).ToArray());  // Span in real impl

        // ← Domain method called from Application layer
        shipment.Seal(cmd.FinalStatus, mkt, cmd.QualityDecision,
            cmd.SignedById.ToString(), cmd.SignedByName, cmd.ReasonForChange);

        await store.AppendAsync(shipment.Id, "Shipment", shipment.UncommittedEvents, shipment.Version - 1, ct);
        shipment.ClearUncommitted();
        await pub.PublishAsync(new ShipmentSealed(cmd.ShipmentId, cmd.TenantId, cmd.FinalStatus), ct);
    }
}
```

---

## 7. Clean Architecture Layer Rules (NetArchTest enforces)

```
Domain:
  ✅ Can reference: nothing external
  ❌ Cannot reference: Npgsql, EF Core, Azure, System.Net.Http, System.Text.Json

Application:
  ✅ Can reference: Domain
  ❌ Cannot reference: Npgsql, Azure.Messaging.ServiceBus, StackExchange.Redis

Infrastructure:
  ✅ Can reference: Application, Domain, all packages
  ❌ Cannot reference: API layer (no circular dependency)

API:
  ✅ Can reference: Application (for handler injection), Infrastructure (for DI registration only)
  ❌ Cannot reference: Domain directly (except DTOs/Contracts)
  ❌ Should not contain business logic

Contracts:
  ✅ Can reference: nothing (consumed by other Cells)
  ❌ Cannot reference: Domain (Contracts are the public API of the Cell)
```

---

## 8. Event Versioning (how to evolve domain events without breaking consumers)

```csharp
// Version 1 — initial event
public record AssetRegistered_V1(
    Guid   AssetId,
    string Name,
    Guid   TenantId
) : DomainEvent { public string EventType => "AssetRegistered_V1"; }

// Version 2 — added SerialNumber (non-breaking: deserializer fills null for old records)
public record AssetRegistered_V2(
    Guid   AssetId,
    string Name,
    Guid   TenantId,
    string? SerialNumber  // nullable — old events won't have it
) : DomainEvent { public string EventType => "AssetRegistered_V2"; }

// Deserializer dispatch table handles both versions:
private static DomainEvent DeserializeEvent(LedgerEntry entry) => entry.EventType switch
{
    "AssetRegistered_V1" => JsonSerializer.Deserialize(entry.PayloadJson, ...V1)!
                             .Migrate(),  // V1 → V2 migration method
    "AssetRegistered_V2" => JsonSerializer.Deserialize(entry.PayloadJson, ...V2)!,
    _ => throw new NotSupportedException(entry.EventType)
};

// Migration extension
public static AssetRegistered_V2 Migrate(this AssetRegistered_V1 v1) =>
    new(v1.AssetId, v1.Name, v1.TenantId, SerialNumber: null);
```
