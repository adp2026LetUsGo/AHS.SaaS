---
name: cell-integration-patterns
description: >
  Expert guidance on cross-cell integration patterns for AHS: Outbox Pattern, Saga,
  compensating transactions, Service Bus topics/subscriptions, testing cross-cell flows
  locally, and event schema versioning in AHS.Cell.*.Contracts. Use this skill whenever
  the user mentions cross-cell communication, Service Bus topics, Outbox Pattern,
  Saga pattern, distributed transaction, compensating transaction, eventual consistency,
  Service Bus subscription, local cross-cell testing, or integration between two AHS Cells.
  Trigger on: cross-cell, Outbox Pattern, Saga, Service Bus topic, subscription filter,
  compensating transaction, eventual consistency, distributed, inter-cell, integration test
  cross-cell, cell event, ICellEvent, Contracts project.
---

# Cell Integration Patterns — C2 Reference
## AHS Ecosystem / Blueprint V3.1

---

## 1. The Three Patterns — When to Use Each

```
SIMPLE EVENT (fire and forget)
  Use when: Cell A announces something happened, doesn't care who listens.
  Example: ShipmentExcursionDetected → multiple consumers react independently.
  Guarantee: At-least-once (Service Bus).
  Code: ICellEvent → CellEventPublisher → Service Bus topic.

OUTBOX PATTERN (reliable delivery)
  Use when: You MUST publish the event if and only if the DB transaction commits.
  Example: RegisterAsset must atomically write to DB AND publish AssetRegistered.
  Without Outbox: DB commits but Service Bus publish fails → inconsistency.
  With Outbox: event goes to DB table in same transaction → background worker publishes.

SAGA (multi-cell process)
  Use when: A business process spans multiple Cells and needs rollback if any step fails.
  Example: New shipment → reserve asset → allocate cold storage → notify carrier.
          If carrier notification fails → compensate (release storage, release asset).
  Complexity: High. Use only when simple events are insufficient.
```

---

## 2. ICellEvent — The Published Language

```csharp
// AHS.Cell.[Name].Contracts — the ONLY project shared between Cells
// Other Cells reference ONLY this project (not Domain, Application, or Infrastructure)

namespace AHS.Cell.[Name].Contracts;

// Marker interface — all published events implement this
public interface ICellEvent
{
    Guid          EventId    { get; }
    string        TenantSlug { get; }
    DateTimeOffset OccurredAt { get; }
    string        CellName   { get; }  // "ColdChain", "AssetManager", etc.
}

// Example published event
public record ShipmentExcursionDetected(
    Guid          EventId,
    string        TenantSlug,
    DateTimeOffset OccurredAt,
    Guid          ShipmentId,
    string        ZoneId,
    double        ObservedCelsius,
    string        Severity           // "Warning" | "Critical" — string, not enum (stable across versions)
) : ICellEvent
{
    public string CellName => "ColdChain";
}

// JsonSerializerContext for Contracts (needed by both publisher and subscriber)
[JsonSerializable(typeof(ShipmentExcursionDetected))]
[JsonSerializable(typeof(AssetRegistered))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class ColdChainContractsJsonContext : JsonSerializerContext { }
```

---

## 3. Service Bus Topic Architecture

```
Topic per Cell: ahs.[cellname].events
  → All events from ColdChain Cell go to: ahs.coldchain.events
  → Subscribers create their own filtered subscriptions

Subscription per consumer Cell:
  ahs.coldchain.events / assetmanager-sub   (filtered: subject = 'ShipmentExcursionDetected')
  ahs.coldchain.events / fintracker-sub     (filtered: subject = 'ShipmentSealed')
  ahs.coldchain.events / controltower-sub   (no filter — receives all)

Filter syntax (SQL-like on message properties):
  subject = 'ShipmentExcursionDetected' AND severity = 'Critical'
```

```csharp
// Publisher — standardized for all Cells
public class ServiceBusCellEventPublisher(ServiceBusClient sb, ITenantContext tenant)
    : ICellEventPublisher
{
    public async Task PublishAsync(ICellEvent evt, CancellationToken ct)
    {
        var topicName = $"ahs.{evt.CellName.ToLower()}.events";
        await using var sender = sb.CreateSender(topicName);

        var json = SerializeEvent(evt);  // AOT-safe dispatch table
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(json))
        {
            MessageId  = evt.EventId.ToString(),
            Subject    = evt.GetType().Name,       // used for subscription filters
            ContentType = "application/json",
            ApplicationProperties =
            {
                ["tenant_slug"] = tenant.TenantSlug,
                ["cell_name"]   = evt.CellName,
                ["occurred_at"] = evt.OccurredAt.ToString("O"),
            }
        };

        await sender.SendMessageAsync(message, ct);
    }

    // AOT-safe — explicit dispatch, no reflection
    private static string SerializeEvent(ICellEvent evt) => evt switch
    {
        ShipmentExcursionDetected e => JsonSerializer.Serialize(e,
            ColdChainContractsJsonContext.Default.ShipmentExcursionDetected),
        ShipmentSealed e => JsonSerializer.Serialize(e,
            ColdChainContractsJsonContext.Default.ShipmentSealed),
        _ => throw new NotSupportedException($"Unknown event: {evt.GetType().Name}")
    };
}
```

---

## 4. Outbox Pattern (Reliable Delivery)

```csharp
// The Outbox guarantees: event is published IF AND ONLY IF the DB transaction commits
// Without Outbox: write to DB + publish to SB = 2 operations, not atomic

// Step 1: Outbox table (same DB as the Cell)
// Migration SQL:
// CREATE TABLE outbox_messages (
//     id           UUID        NOT NULL DEFAULT gen_random_uuid(),
//     tenant_id    UUID        NOT NULL,
//     topic        VARCHAR(100) NOT NULL,
//     subject      VARCHAR(200) NOT NULL,
//     payload_json JSONB       NOT NULL,
//     created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
//     published_at TIMESTAMPTZ,
//     CONSTRAINT pk_outbox PRIMARY KEY (id)
// );
// CREATE INDEX ix_outbox_unpublished ON outbox_messages (created_at) WHERE published_at IS NULL;

// Step 2: Write to Outbox IN the same transaction as the domain event
public class RegisterAssetHandler(AssetDbContext db, IEventStore store)
{
    public async Task<Guid> HandleAsync(RegisterAssetCommand cmd, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var asset = Asset.Create(cmd.Name, cmd.Category, cmd.TenantId,
            cmd.SignedById, cmd.SignedByName, cmd.ReasonForChange);

        // Write domain events to GxP Ledger
        await store.AppendAsync(asset.Id, "Asset", asset.UncommittedEvents, 0, ct);

        // Write to Outbox — same transaction
        var outboxEntry = new OutboxMessage(
            Id:          Guid.NewGuid(),
            TenantId:    cmd.TenantId,
            Topic:       "ahs.assetmanager.events",
            Subject:     nameof(AssetRegistered),
            PayloadJson: JsonSerializer.Serialize(
                new AssetRegistered(asset.Id, cmd.TenantId, cmd.Name, cmd.Category),
                AssetManagerContractsJsonContext.Default.AssetRegistered));

        db.Set<OutboxMessage>().Add(outboxEntry);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);  // BOTH ledger + outbox committed atomically

        asset.ClearUncommitted();
        return asset.Id;
    }
}

// Step 3: Background worker publishes Outbox messages to Service Bus
public class OutboxPublisherWorker(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db  = scope.ServiceProvider.GetRequiredService<AssetDbContext>();
            var pub = scope.ServiceProvider.GetRequiredService<ServiceBusClient>();

            var pending = await db.Set<OutboxMessage>()
                .Where(m => m.PublishedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(20)  // process in batches
                .ToListAsync(ct);

            foreach (var msg in pending)
            {
                await PublishToServiceBusAsync(pub, msg, ct);
                msg.PublishedAt = DateTimeOffset.UtcNow;
            }

            if (pending.Count > 0) await db.SaveChangesAsync(ct);
            await Task.Delay(TimeSpan.FromSeconds(5), ct);  // poll every 5s
        }
    }
}
```

---

## 5. Saga Pattern (Multi-Cell Process)

```csharp
// Use Saga only when a business process spans Cells and needs compensation
// Keep Sagas in the orchestrating Cell (Control Tower or the initiating Cell)

// Example: New Shipment Saga
// Step 1: Create shipment (ColdChain)
// Step 2: Reserve asset (AssetManager)
// Step 3: If Step 2 fails → compensate Step 1 (cancel shipment)

public class NewShipmentSaga(
    IColdChainClient coldChain,
    IAssetManagerClient assetManager,
    IGxPLedger ledger)
{
    public async Task<SagaResult> ExecuteAsync(StartShipmentSagaCommand cmd, CancellationToken ct)
    {
        Guid? shipmentId = null;

        try
        {
            // Step 1
            shipmentId = await coldChain.CreateShipmentAsync(cmd.ShipmentDetails, ct);

            // Step 2
            await assetManager.ReserveAssetAsync(cmd.AssetId, shipmentId.Value, ct);

            // All steps succeeded
            await ledger.AppendAsync(new SagaCompleted(cmd.SagaId, "NewShipment",
                new[] { shipmentId.Value.ToString() }));

            return SagaResult.Success(shipmentId.Value);
        }
        catch (AssetNotAvailableException ex)
        {
            // Compensate Step 1
            if (shipmentId.HasValue)
                await coldChain.CancelShipmentAsync(shipmentId.Value,
                    reason: $"Saga compensation: asset unavailable — {ex.Message}", ct);

            await ledger.AppendAsync(new SagaCompensated(cmd.SagaId, "NewShipment", ex.Message));
            return SagaResult.Compensated(ex.Message);
        }
    }
}
```

---

## 6. Testing Cross-Cell Flows Locally

```csharp
// Strategy: Use InMemoryCellEventBus in local tests — no Service Bus emulator needed for unit tests
// Service Bus emulator for integration tests only

public class InMemoryCellEventBus : ICellEventPublisher, ICellEventSubscriber
{
    private readonly Dictionary<Type, List<Func<ICellEvent, CancellationToken, Task>>> _handlers = new();

    public Task PublishAsync(ICellEvent evt, CancellationToken ct)
    {
        if (_handlers.TryGetValue(evt.GetType(), out var handlers))
            return Task.WhenAll(handlers.Select(h => h(evt, ct)));
        return Task.CompletedTask;
    }

    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : ICellEvent
    {
        if (!_handlers.ContainsKey(typeof(TEvent)))
            _handlers[typeof(TEvent)] = [];
        _handlers[typeof(TEvent)].Add((evt, ct) => handler((TEvent)evt, ct));
    }
}

// Cross-cell integration test using InMemoryCellEventBus
public class CrossCellIntegrationTest
{
    [Fact]
    public async Task Excursion_in_ColdChain_flags_asset_in_AssetManager()
    {
        var bus = new InMemoryCellEventBus();
        var coldChainHandler  = BuildColdChainHandlers(bus);
        var assetManagerProjection = BuildAssetManagerProjection(bus);

        // ColdChain detects excursion and publishes event
        await coldChainHandler.DetectExcursionAsync(shipmentId, sensorReading);

        // AssetManager reacts (same in-memory bus)
        var assetStatus = await assetManagerProjection.GetStatusAsync(assetId);

        assetStatus.Should().Be("AtRisk",
            because: "AssetManager must react to ColdChain excursion event");
    }
}
```

---

## 7. Cell Contracts Versioning

```
Rule: Contracts are append-only. Never remove or rename a field.
      Old consumers break if you remove a field they depend on.

Versioning strategy:
  Breaking change → new record type with version suffix
    AssetRegistered_V1 → AssetRegistered_V2 (old consumers still receive V1)
    Transition period: publish BOTH V1 and V2 during migration window
    Deprecation: remove V1 publisher after all consumers migrate

Non-breaking change → add nullable field to existing record
    Old consumers ignore the new field (JSON deserialization is lenient)
    New consumers use the new field
    No version bump needed

Never do:
  ❌ Rename a field (breaks all consumers silently — JSON ignores unknown field)
  ❌ Change a field type (int → string: breaks deserialization)
  ❌ Remove a required field (breaks deserialization in strict mode)
  ❌ Change the EventType string (breaks subscription filters)
```

---

## 8. Local Development Stack for Cross-Cell Testing

```yaml
# docker-compose.yml additions for cross-cell local dev
services:
  servicebus-emulator:
    image: mcr.microsoft.com/azure-messaging/servicebus-emulator:latest
    ports: ["5672:5672", "5300:5300"]  # AMQP + management
    environment:
      ACCEPT_EULA: "Y"
    volumes:
      - ./infra/local/servicebus-config.json:/ServiceBus_Emulator/ConfigFiles/Config.json

  # Cell A
  coldchain-api:
    build: AHS.Cell.ColdChain
    environment:
      ServiceBus__ConnectionString: "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=..."
    depends_on: [postgres, redis, servicebus-emulator]

  # Cell B — subscribes to Cell A's events
  assetmanager-api:
    build: AHS.Cell.AssetManager
    environment:
      ServiceBus__ConnectionString: "..."
      ServiceBus__ColdChainTopic: "ahs.coldchain.events"
      ServiceBus__ColdChainSubscription: "assetmanager-sub"
    depends_on: [postgres, redis, servicebus-emulator, coldchain-api]
```

```json
// infra/local/servicebus-config.json — pre-create topics and subscriptions
{
  "Namespaces": [{
    "Name": "ahs-local",
    "Queues": [],
    "Topics": [
      {
        "Name": "ahs.coldchain.events",
        "Subscriptions": [
          { "Name": "assetmanager-sub", "Rules": [{ "Filter": "subject = 'ShipmentExcursionDetected'" }] },
          { "Name": "controltower-sub",  "Rules": [{ "Filter": "1=1" }] }
        ]
      },
      {
        "Name": "ahs.assetmanager.events",
        "Subscriptions": [
          { "Name": "controltower-sub", "Rules": [{ "Filter": "1=1" }] }
        ]
      }
    ]
  }]
}
```
