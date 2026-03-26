---
name: industrial-cold-chain-logic
description: >
  Expert guidance on industrial cold chain management systems in C#, including temperature
  monitoring, excursion detection, HACCP compliance, cold chain events, sensor integration,
  alerting, data modeling for refrigerated transport/storage, and pharmaceutical/food
  cold chain regulations. Use this skill whenever the user mentions cold chain, temperature
  monitoring, excursion, HACCP, refrigerated transport, frozen storage, sensor readings,
  temperature logger, cold storage, temperature alert, MKT (Mean Kinetic Temperature),
  pharmaceutical cold chain, 2-8°C, -20°C, or cold room monitoring.
  Trigger on: cold chain, temperature excursion, HACCP, MKT, cold storage, temperature sensor,
  refrigerated, frozen transport, temperature alert, cold room, 2-8 degrees.
---

# Industrial Cold Chain Logic in C#

## Domain Vocabulary

| Term | Meaning |
|---|---|
| **Excursion** | Temperature outside defined range for longer than allowed duration |
| **HACCP** | Hazard Analysis and Critical Control Points (FDA/EU regulatory framework) |
| **MKT** | Mean Kinetic Temperature — effective temperature accounting for Arrhenius kinetics |
| **Critical Limit** | Temperature boundary that must never be exceeded |
| **CCP** | Critical Control Point — monitoring checkpoint in the chain |
| **Setpoint** | Target temperature for a zone |
| **Alarm Delay** | Grace period before alert fires (avoids false positives on door opens) |

---

## 0. AOT / Trim Setup Requerido

> En **Native AOT**, registrar todos los tipos del dominio en un `JsonSerializerContext`:

```csharp
[JsonSerializable(typeof(SensorPayload))]
[JsonSerializable(typeof(ColdChainReport))]
[JsonSerializable(typeof(TemperatureExcursion))]
[JsonSerializable(typeof(List<TemperatureExcursion>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class AppJsonContext : JsonSerializerContext { }
```

> **EF Core + AOT:** `ModelBuilder.Model.GetEntityTypes()` con `MakeGenericMethod` rompe en AOT. Usar `HasQueryFilter` por entidad explícita (ver skill multitenancy).

---

```csharp
// Temperature reading from a sensor
public record TemperatureReading(
    Guid        SensorId,
    string      ZoneId,
    DateTimeOffset Timestamp,
    double      CelsiusValue,
    SensorStatus Status = SensorStatus.Ok);

public enum SensorStatus { Ok, Fault, Disconnected, Calibrating }

// Zone configuration
public record TemperatureZone(
    string ZoneId,
    string Name,
    double MinCelsius,       // e.g. 2.0
    double MaxCelsius,       // e.g. 8.0
    TimeSpan AlarmDelay,     // grace period before alert fires
    TimeSpan MaxExcursionDuration,  // max cumulative excursion allowed
    ZoneType Type = ZoneType.Chilled);

public enum ZoneType { Ambient, Chilled, Frozen, DeepFrozen, UltraLow }

// Predefined profiles
public static class ZoneProfiles
{
    public static TemperatureZone Pharmaceutical2to8 => new(
        "pharma-2-8", "Pharma 2–8°C",
        MinCelsius: 2.0, MaxCelsius: 8.0,
        AlarmDelay: TimeSpan.FromMinutes(15),
        MaxExcursionDuration: TimeSpan.FromHours(1));

    public static TemperatureZone Frozen => new(
        "frozen", "Frozen -25 to -15°C",
        MinCelsius: -25.0, MaxCelsius: -15.0,
        AlarmDelay: TimeSpan.FromMinutes(30),
        MaxExcursionDuration: TimeSpan.FromHours(2));

    public static TemperatureZone DeepFrozen => new(
        "deep-frozen", "Deep Frozen ≤ -70°C",
        MinCelsius: -90.0, MaxCelsius: -70.0,
        AlarmDelay: TimeSpan.FromMinutes(10),
        MaxExcursionDuration: TimeSpan.FromMinutes(30));
}

// Excursion event
public record TemperatureExcursion(
    Guid       ExcursionId,
    string     ZoneId,
    Guid       SensorId,
    DateTimeOffset StartedAt,
    DateTimeOffset? ResolvedAt,
    double     MinObserved,
    double     MaxObserved,
    ExcursionSeverity Severity,
    ExcursionType Type);

public enum ExcursionSeverity { Warning, Critical, Fatal }
public enum ExcursionType     { High, Low, SensorFault }
```

---

## 2. Excursion Detection Engine

```csharp
public class ExcursionDetector(TemperatureZone zone, ILogger<ExcursionDetector> logger)
{
    private readonly Queue<TemperatureReading> _window = new();
    private TemperatureExcursion? _activeExcursion;
    private DateTimeOffset? _excursionStartedAt;

    public ExcursionDetectionResult Process(TemperatureReading reading)
    {
        if (reading.Status != SensorStatus.Ok)
            return HandleSensorFault(reading);

        bool isOutOfRange = reading.CelsiusValue < zone.MinCelsius
                         || reading.CelsiusValue > zone.MaxCelsius;

        if (!isOutOfRange)
        {
            _excursionStartedAt = null;

            if (_activeExcursion != null)
            {
                var resolved = _activeExcursion with { ResolvedAt = reading.Timestamp };
                _activeExcursion = null;
                return ExcursionDetectionResult.Resolved(resolved);
            }
            return ExcursionDetectionResult.Normal(reading);
        }

        // Out of range — check alarm delay
        _excursionStartedAt ??= reading.Timestamp;
        var duration = reading.Timestamp - _excursionStartedAt.Value;

        if (duration < zone.AlarmDelay)
            return ExcursionDetectionResult.Pending(reading, duration);

        // Alarm delay exceeded — raise or update excursion
        var severity = ClassifySeverity(reading.CelsiusValue, duration);

        if (_activeExcursion == null)
        {
            _activeExcursion = new TemperatureExcursion(
                ExcursionId:   Guid.NewGuid(),
                ZoneId:        zone.ZoneId,
                SensorId:      reading.SensorId,
                StartedAt:     _excursionStartedAt.Value,
                ResolvedAt:    null,
                MinObserved:   reading.CelsiusValue,
                MaxObserved:   reading.CelsiusValue,
                Severity:      severity,
                Type:          reading.CelsiusValue > zone.MaxCelsius ? ExcursionType.High : ExcursionType.Low);

            logger.LogWarning("Excursion started: Zone={Zone}, Temp={Temp:F1}°C, Severity={Severity}",
                zone.ZoneId, reading.CelsiusValue, severity);

            return ExcursionDetectionResult.ExcursionStarted(_activeExcursion);
        }

        // Update active excursion stats
        _activeExcursion = _activeExcursion with
        {
            MinObserved = Math.Min(_activeExcursion.MinObserved, reading.CelsiusValue),
            MaxObserved = Math.Max(_activeExcursion.MaxObserved, reading.CelsiusValue),
            Severity    = severity,
        };

        return ExcursionDetectionResult.ExcursionOngoing(_activeExcursion, reading);
    }

    private ExcursionSeverity ClassifySeverity(double celsius, TimeSpan duration)
    {
        bool criticalTemp = celsius > zone.MaxCelsius + 5 || celsius < zone.MinCelsius - 5;
        bool criticalTime = duration > zone.MaxExcursionDuration;

        return (criticalTemp || criticalTime) ? ExcursionSeverity.Critical : ExcursionSeverity.Warning;
    }

    private ExcursionDetectionResult HandleSensorFault(TemperatureReading reading)
    {
        logger.LogError("Sensor fault: SensorId={Id}, Status={Status}", reading.SensorId, reading.Status);
        return ExcursionDetectionResult.SensorFault(reading);
    }
}
```

---

## 3. Mean Kinetic Temperature (MKT)

```csharp
/// <summary>
/// MKT accounts for degradation acceleration at higher temperatures (Arrhenius model).
/// FDA/ICH Q1A standard for stability assessment.
/// </summary>
public static class MeanKineticTemperature
{
    // Activation energy (J/mol) for most pharmaceutical products
    private const double DefaultActivationEnergy = 83_144.0; // J/mol
    private const double GasConstant = 8.314;                // J/(mol·K)

    /// <param name="readings">Temperature readings in °C</param>
    /// <param name="activationEnergy">Ea in J/mol (default 83144 = FDA standard)</param>
    public static double Calculate(
        IReadOnlyList<double> readings,
        double activationEnergy = DefaultActivationEnergy)
    {
        if (readings.Count == 0) throw new ArgumentException("No readings provided.");

        // Σ exp(-Ea / R·Tk) / n
        double sumExp = readings
            .Select(c => c + 273.15) // °C → Kelvin
            .Sum(tk => Math.Exp(-activationEnergy / (GasConstant * tk)));

        double avgExp = sumExp / readings.Count;

        // MKT (K) = -Ea/R / ln(avgExp), then convert back to °C
        double mktKelvin = -activationEnergy / (GasConstant * Math.Log(avgExp));
        return mktKelvin - 273.15;
    }

    /// <summary>Determine if MKT exceeds label storage condition.</summary>
    public static bool IsWithinLimit(IReadOnlyList<double> readings, double maxCelsius,
        double activationEnergy = DefaultActivationEnergy)
        => Calculate(readings, activationEnergy) <= maxCelsius;
}
```

---

## 4. Cold Chain Report Generator

```csharp
public class ColdChainReport
{
    public Guid       ShipmentId       { get; init; }
    public string     ZoneId           { get; init; } = "";
    public DateTimeOffset StartTime    { get; init; }
    public DateTimeOffset EndTime      { get; init; }
    public double     MinTemp          { get; init; }
    public double     MaxTemp          { get; init; }
    public double     MeanKineticTemp  { get; init; }
    public double     SetpointMin      { get; init; }
    public double     SetpointMax      { get; init; }
    public TimeSpan   TotalDuration    { get; init; }
    public TimeSpan   TimeOutOfRange   { get; init; }
    public int        ExcursionCount   { get; init; }
    public bool       IsCompliant      { get; init; }
    public string     ComplianceNote   { get; init; } = "";
    public IReadOnlyList<TemperatureExcursion> Excursions { get; init; } = [];

    public static ColdChainReport Build(
        Guid shipmentId,
        TemperatureZone zone,
        IReadOnlyList<TemperatureReading> readings,
        IReadOnlyList<TemperatureExcursion> excursions)
    {
        var validReadings = readings.Where(r => r.Status == SensorStatus.Ok).ToList();
        var temps = validReadings.Select(r => r.CelsiusValue).ToList();

        var mkt  = temps.Count > 0 ? MeanKineticTemperature.Calculate(temps) : double.NaN;
        var oor  = excursions.Sum(e =>
            (e.ResolvedAt ?? readings.Last().Timestamp) - e.StartedAt);

        bool compliant = excursions.All(e => e.Severity != ExcursionSeverity.Critical)
                      && (zone.ZoneId.Contains("pharma") ? mkt <= zone.MaxCelsius : true);

        return new ColdChainReport
        {
            ShipmentId      = shipmentId,
            ZoneId          = zone.ZoneId,
            StartTime       = readings.First().Timestamp,
            EndTime         = readings.Last().Timestamp,
            MinTemp         = temps.Count > 0 ? temps.Min() : double.NaN,
            MaxTemp         = temps.Count > 0 ? temps.Max() : double.NaN,
            MeanKineticTemp = mkt,
            SetpointMin     = zone.MinCelsius,
            SetpointMax     = zone.MaxCelsius,
            TotalDuration   = readings.Last().Timestamp - readings.First().Timestamp,
            TimeOutOfRange  = oor,
            ExcursionCount  = excursions.Count,
            IsCompliant     = compliant,
            ComplianceNote  = compliant ? "Within specification." : "EXCURSION — Product review required.",
            Excursions      = excursions,
        };
    }
}
```

---

## 5. Real-Time Monitoring Pipeline (Channel)

```csharp
public class TemperatureMonitoringPipeline(
    IEnumerable<TemperatureZone> zones,
    IAlertService alertService,
    IExcursionRepository repo)
{
    private readonly Channel<TemperatureReading> _channel =
        Channel.CreateBounded<TemperatureReading>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest, // never block sensor writes
            SingleReader = false,
            SingleWriter = false,
        });

    private readonly ConcurrentDictionary<string, ExcursionDetector> _detectors = new(
        zones.ToDictionary(z => z.ZoneId, z => new ExcursionDetector(z, /* logger */ null!)));

    // Called by sensor adapters / MQTT subscribers
    public ValueTask IngestAsync(TemperatureReading reading)
        => _channel.Writer.WriteAsync(reading);

    // Run as hosted service
    public async Task ProcessAsync(CancellationToken ct)
    {
        await foreach (var reading in _channel.Reader.ReadAllAsync(ct))
        {
            if (!_detectors.TryGetValue(reading.ZoneId, out var detector)) continue;

            var result = detector.Process(reading);

            if (result.Type == DetectionResultType.ExcursionStarted)
            {
                await repo.SaveExcursionAsync(result.Excursion!, ct);
                await alertService.SendExcursionAlertAsync(result.Excursion!, ct);
            }
            else if (result.Type == DetectionResultType.Resolved)
            {
                await repo.ResolveExcursionAsync(result.Excursion!.ExcursionId, reading.Timestamp, ct);
                await alertService.SendResolutionNoticeAsync(result.Excursion!, ct);
            }
        }
    }
}
```

---

## 6. Sensor Ingestion — Patrón Adaptador Multi-Protocolo

> AHS es **agnóstico al protocolo de sensores**. Los clientes eligen sus propios data loggers
> y brokers (Testo, Sensitech, Monnit, ELPRO, Corintech, etc.).
> AHS expone un **contrato de entrada** — `ISensorAdapter` — y publica en Azure Service Bus.
> El pipeline de monitoreo consume siempre desde Service Bus, sin importar el origen.

```
Cliente elige:              AHS adapta:            AHS procesa:
──────────────────         ─────────────────       ──────────────────────
MQTT broker          →     MqttSensorAdapter  →┐
HTTP POST webhook    →     HttpSensorAdapter  →┤  Azure Service Bus
Azure IoT Hub        →     IoTHubSensorAdapter→┤  (sensor-readings topic)
OPC-UA server        →     OpcUaSensorAdapter →┤       ↓
Modbus TCP           →     ModbusSensorAdapter→┘  TemperatureMonitoringPipeline
CSV/FTP upload       →     FileSensorAdapter  →┘
```

### Contrato de entrada — `ISensorAdapter`

```csharp
// Cada adaptador implementa esta única interfaz
// AHS no impone el protocolo al cliente
public interface ISensorAdapter
{
    string ProtocolName { get; }   // "MQTT" | "HTTP" | "IoTHub" | "OPC-UA" | "Modbus" | "File"
    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}

// Normalized reading — formato interno AHS, independiente del protocolo
public record NormalizedSensorReading(
    string RawDeviceId,    // ID del dispositivo tal como lo envía el cliente
    string AhsSensorId,    // mapeado al sensor AHS (resuelto por DeviceRegistry)
    string ZoneId,
    DateTimeOffset Timestamp,
    double CelsiusValue,
    string ProtocolSource, // auditoría: de dónde vino el dato
    IReadOnlyDictionary<string, string> RawMetadata); // payload original sin procesar
```

### Device Registry — mapeo dispositivo cliente → sensor AHS

```csharp
// Los clientes registran sus dispositivos en el portal AHS
// El adaptador resuelve el ID antes de publicar
public interface IDeviceRegistry
{
    Task<string?> ResolveAhsSensorIdAsync(string rawDeviceId, Guid tenantId, CancellationToken ct);
    Task<string?> ResolveZoneIdAsync(string rawDeviceId, Guid tenantId, CancellationToken ct);
}

public class PostgresDeviceRegistry(IDbConnectionFactory db, ITenantContext tenant)
    : IDeviceRegistry
{
    public async Task<string?> ResolveAhsSensorIdAsync(string rawDeviceId, Guid tenantId, CancellationToken ct)
    {
        await using var conn = await db.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<string?>(
            "SELECT ahs_sensor_id FROM device_mappings WHERE raw_device_id = @raw AND tenant_id = @tid",
            new { raw = rawDeviceId, tid = tenantId });
    }

    public async Task<string?> ResolveZoneIdAsync(string rawDeviceId, Guid tenantId, CancellationToken ct)
    {
        await using var conn = await db.CreateAsync(ct);
        return await conn.ExecuteScalarAsync<string?>(
            "SELECT zone_id FROM device_mappings WHERE raw_device_id = @raw AND tenant_id = @tid",
            new { raw = rawDeviceId, tid = tenantId });
    }
}
```

### Azure Service Bus Publisher — nexo entre adaptadores y pipeline

```csharp
// Todos los adaptadores publican aquí — el pipeline consume desde aquí
public class SensorEventPublisher(ServiceBusClient serviceBus, ITenantContext tenant,
    ILogger<SensorEventPublisher> logger)
{
    private readonly ServiceBusSender _sender =
        serviceBus.CreateSender("ahs.sensor-readings");

    public async Task PublishAsync(NormalizedSensorReading reading, CancellationToken ct)
    {
        // AOT-safe serialization
        var json = JsonSerializer.Serialize(reading, ColdChainJsonContext.Default.NormalizedSensorReading);

        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(json))
        {
            MessageId      = $"{reading.AhsSensorId}:{reading.Timestamp:O}",
            Subject        = "temperature-reading",
            ApplicationProperties =
            {
                ["tenant_id"]       = tenant.TenantId.ToString(),
                ["zone_id"]         = reading.ZoneId,
                ["protocol_source"] = reading.ProtocolSource,
            }
        };

        await _sender.SendMessageAsync(message, ct);
        logger.LogDebug("Published reading from {Protocol} sensor {Id}",
            reading.ProtocolSource, reading.AhsSensorId);
    }
}
```

### Service Bus Consumer → Pipeline

```csharp
// IHostedService — consume desde Service Bus y alimenta el pipeline
public class SensorReadingConsumer(ServiceBusClient serviceBus,
    TemperatureMonitoringPipeline pipeline, ILogger<SensorReadingConsumer> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var processor = serviceBus.CreateProcessor("ahs.sensor-readings",
            new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 16,
                AutoCompleteMessages = false,
                PrefetchCount = 100,
            });

        processor.ProcessMessageAsync += async args =>
        {
            var json = args.Message.Body.ToString();
            var reading = JsonSerializer.Deserialize(json,
                ColdChainJsonContext.Default.NormalizedSensorReading);

            if (reading is null)
            {
                await args.DeadLetterMessageAsync(args.Message, "InvalidPayload",
                    "Cannot deserialize NormalizedSensorReading", ct);
                return;
            }

            await pipeline.IngestAsync(new TemperatureReading(
                SensorId:     Guid.Parse(reading.AhsSensorId),
                ZoneId:       reading.ZoneId,
                Timestamp:    reading.Timestamp,
                CelsiusValue: reading.CelsiusValue), ct);

            await args.CompleteMessageAsync(args.Message, ct);
        };

        processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Service Bus error on {Source}", args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(ct);
        await Task.Delay(Timeout.Infinite, ct);
        await processor.StopProcessingAsync();
    }
}
```

### Cache — HybridCache para Device Registry

```csharp
// Device mappings son estables — candidato ideal para HybridCache
// HybridCache (.NET 9/10): L1 IMemoryCache + L2 Redis, AOT-safe
public class CachedDeviceRegistry(IDeviceRegistry inner, HybridCache cache) : IDeviceRegistry
{
    public async Task<string?> ResolveAhsSensorIdAsync(string rawDeviceId, Guid tenantId, CancellationToken ct)
        => await cache.GetOrCreateAsync(
            $"device:{tenantId}:{rawDeviceId}:sensor",
            async token => await inner.ResolveAhsSensorIdAsync(rawDeviceId, tenantId, token),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(1) },
            cancellationToken: ct);

    public async Task<string?> ResolveZoneIdAsync(string rawDeviceId, Guid tenantId, CancellationToken ct)
        => await cache.GetOrCreateAsync(
            $"device:{tenantId}:{rawDeviceId}:zone",
            async token => await inner.ResolveZoneIdAsync(rawDeviceId, tenantId, token),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(1) },
            cancellationToken: ct);
}

// Program.cs
builder.Services.AddHybridCache();                          // L1: IMemoryCache automático
builder.Services.AddStackExchangeRedisCache(o =>            // L2: Redis
    o.Configuration = builder.Configuration["Redis:ConnectionString"]);
builder.Services.AddScoped<IDeviceRegistry, PostgresDeviceRegistry>();
builder.Services.Decorate<IDeviceRegistry, CachedDeviceRegistry>(); // Scrutor
```

### Adaptador de ejemplo — HTTP Webhook (el más común para data loggers cloud)

```csharp
// Minimal API endpoint — los data loggers POST sus lecturas aquí
// El cliente configura su data logger para enviar a: POST /api/ingest/{tenantSlug}
app.MapPost("/api/ingest/{tenantSlug}", async (
    string tenantSlug,
    HttpSensorPayload payload,        // formato negociado con el cliente
    IDeviceRegistry registry,
    SensorEventPublisher publisher,
    ITenantContext tenant,
    CancellationToken ct) =>
{
    var sensorId = await registry.ResolveAhsSensorIdAsync(payload.DeviceId, tenant.TenantId, ct);
    if (sensorId is null) return Results.NotFound($"Device '{payload.DeviceId}' not registered.");

    var zoneId = await registry.ResolveZoneIdAsync(payload.DeviceId, tenant.TenantId, ct);

    await publisher.PublishAsync(new NormalizedSensorReading(
        RawDeviceId:    payload.DeviceId,
        AhsSensorId:    sensorId,
        ZoneId:         zoneId ?? "unknown",
        Timestamp:      payload.Timestamp ?? DateTimeOffset.UtcNow,
        CelsiusValue:   payload.TemperatureCelsius,
        ProtocolSource: "HTTP",
        RawMetadata:    payload.Metadata ?? new Dictionary<string, string>()), ct);

    return Results.Accepted();
})
.WithName("IngestSensorReading")
.WithSummary("Universal sensor ingestion endpoint — protocol-agnostic");
```

---

## 7. HACCP Compliance Checklist (Code as Policy)

```csharp
public class HaccpComplianceValidator
{
    public HaccpValidationResult Validate(ColdChainReport report)
    {
        var violations = new List<string>();

        // Critical limits — must never be breached
        if (report.MaxTemp > report.SetpointMax + 10)
            violations.Add($"Critical limit exceeded: {report.MaxTemp:F1}°C (limit: {report.SetpointMax + 10:F1}°C)");

        if (report.MinTemp < report.SetpointMin - 10)
            violations.Add($"Critical low limit breached: {report.MinTemp:F1}°C");

        // Cumulative excursion time
        if (report.TimeOutOfRange > TimeSpan.FromHours(2))
            violations.Add($"Total excursion time {report.TimeOutOfRange.TotalMinutes:F0} min exceeds 120 min limit");

        // MKT check for pharma products
        if (!double.IsNaN(report.MeanKineticTemp) && report.MeanKineticTemp > report.SetpointMax)
            violations.Add($"MKT {report.MeanKineticTemp:F2}°C exceeds storage specification {report.SetpointMax:F1}°C");

        return new HaccpValidationResult(
            IsCompliant: violations.Count == 0,
            Violations: violations,
            Disposition: violations.Count == 0 ? "ACCEPT" : "QUARANTINE — Quality review required");
    }
}

public record HaccpValidationResult(
    bool IsCompliant,
    IReadOnlyList<string> Violations,
    string Disposition);
```

---

## 8. Standard Temperature Ranges Reference

| Product Category | Range | Max Excursion |
|---|---|---|
| Pharmaceutical (refrigerated) | 2–8°C | 24h at 15°C; 2h at 25°C |
| Pharmaceutical (frozen) | ≤ -15°C | Varies by product |
| Pharmaceutical (deep frozen) | ≤ -60°C | 30 min |
| Fresh food (general) | 0–4°C | 2h |
| Frozen food | ≤ -18°C | 30 min above -15°C |
| Ambient controlled | 15–25°C | N/A |
| Cryogenic (LN₂) | ≤ -196°C | Minutes |
