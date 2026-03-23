---
name: logistics-oracle-xai
description: >
  Expert guidance on the AHS Logistics Oracle (REQ-001) — weighted multi-factor risk
  inference engine, Pessimistic TTF calculation, and XAI DNA 14-point diagnostic system
  in C# 14 / .NET 10 / Native AOT. Use this skill whenever the user mentions Logistics
  Oracle, REQ-001, weighted risk profile, route threat, carrier reliability, TTF,
  Time to Failure, Pessimistic TTF, XAI DNA, explainable AI, risk score, logistics risk,
  passive insulation penalty, cold chain forecast, or predictive risk.
  Trigger on: Logistics Oracle, REQ-001, weighted risk, TTF, Time to Failure,
  XAI DNA, explainable AI, logistics risk score, route threat, carrier reliability,
  passive insulation, pessimistic TTF, risk inference engine.
---

# Logistics Oracle & XAI DNA — AHS REQ-001
## C# 14 / .NET 10 / Native AOT

## Blueprint Spec (REQ-001)

```
Weighted Risk Profile:
  Route Threat Level:          40%
  Carrier Reliability:         30%
  External Temperature Forecast: 30%

Critical Modifier:
  Passive Insulation:         +15% base penalty (applied BEFORE weighting)

Pessimistic TTF:
  Reduces physical TTF by LogisticsRiskScore
  "Safe window" for operator intervention
```

---

## 0b. Capa 5 — Performance & Zero-Allocation (P99 < 10ms)

> El Oracle es la ruta crítica del sistema. Capa 5 impone P99 < 10ms.
> Implicaciones técnicas: `ValueTask`, `Span<T>`, sin LINQ en hot path, sin allocations GC-presionantes.

```csharp
// ✅ ValueTask — evita heap allocation cuando el resultado está en caché
public class LogisticsOracle
{
    // Cache de resultados recientes — los mismos parámetros de ruta se repiten
    private readonly HybridCache _cache;

    public async ValueTask<OracleResult> CalculateAsync(OracleRequest req, CancellationToken ct)
    {
        // Cache key determinista — sin string interpolation en hot path
        Span<char> keyBuffer = stackalloc char[128];
        var key = BuildCacheKey(req, keyBuffer);

        // HybridCache: L1 IMemoryCache (sin serialización) → L2 Redis (con JSON)
        return await _cache.GetOrCreateAsync(key.ToString(),
            async token => await ComputeAsync(req, token),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            cancellationToken: ct);
    }

    // Hot path — sin LINQ, sin allocations, sin boxing
    private static double ScoreCarrierZeroAlloc(OracleRequest req)
    {
        // ✅ Sin LINQ (no .Sum(), no .Select(), no .Where())
        // ✅ Sin boxing (double, no object)
        var reliabilityScore = (1.0 - req.CarrierReliability) * 100.0;
        var incidentPenalty  = req.CarrierIncidents12M * 8.0;
        if (incidentPenalty > 40.0) incidentPenalty = 40.0;  // Math.Min → branch más barato en JIT
        return reliabilityScore + incidentPenalty > 100.0 ? 100.0 : reliabilityScore + incidentPenalty;
    }

    // Cache key sin allocations — escribe en Span<char> del stack
    private static ReadOnlySpan<char> BuildCacheKey(OracleRequest req, Span<char> buffer)
    {
        var written = 0;
        "oracle:".AsSpan().CopyTo(buffer[written..]); written += 7;
        req.RouteId.AsSpan().CopyTo(buffer[written..]); written += req.RouteId.Length;
        buffer[written++] = ':';
        req.InsulationType.AsSpan().CopyTo(buffer[written..]); written += req.InsulationType.Length;
        return buffer[..written];
    }
}

// ✅ ThermalDataPoint — Adapter/Port pattern (Capa 5: el Oracle no sabe qué es un "Sensor Modbus")
// El adaptador de sensor convierte a este tipo antes de llegar al Oracle/HPC
public readonly record struct ThermalDataPoint(
    double CelsiusValue,
    DateTimeOffset Timestamp,
    string ZoneId)
{
    // readonly record struct = stack-allocated, zero heap pressure en el hot path
}
```

---

## 0c. Adapter / Port Pattern — Separación Sensor ↔ Dominio

```csharp
// El Oracle recibe ThermalDataPoint, NUNCA un SensorReading con detalles de protocolo
// Esto desacopla AHS.Engines.HPC de cualquier protocolo de sensor

public interface IThermalDataSource
{
    // Puerto — el Oracle habla con este contrato, no con sensores
    IAsyncEnumerable<ThermalDataPoint> StreamAsync(string zoneId, CancellationToken ct);
}

// Adaptador — traduce desde el protocolo del cliente al puerto del dominio
// Vive en AHS.Infrastructure, NO en AHS.Common ni AHS.Engines.HPC
public class ServiceBusThermalAdapter(ServiceBusClient sb, IDeviceRegistry registry)
    : IThermalDataSource
{
    public async IAsyncEnumerable<ThermalDataPoint> StreamAsync(
        string zoneId, [EnumeratorCancellation] CancellationToken ct)
    {
        var processor = sb.CreateProcessor("ahs.sensor-readings");
        var channel   = Channel.CreateBounded<ThermalDataPoint>(1000);

        processor.ProcessMessageAsync += async args =>
        {
            var raw = JsonSerializer.Deserialize(
                args.Message.Body.ToString(),
                ColdChainJsonContext.Default.NormalizedSensorReading);

            if (raw?.ZoneId == zoneId)
                await channel.Writer.WriteAsync(
                    new ThermalDataPoint(raw.CelsiusValue, raw.Timestamp, raw.ZoneId), ct);

            await args.CompleteMessageAsync(args.Message, ct);
        };

        await processor.StartProcessingAsync(ct);

        await foreach (var point in channel.Reader.ReadAllAsync(ct))
            yield return point;
    }
}
```

---

## 0. AOT Setup

```csharp
[JsonSerializable(typeof(OracleRequest))]
[JsonSerializable(typeof(OracleResult))]
[JsonSerializable(typeof(XaiDna))]
[JsonSerializable(typeof(XaiDnaFactor))]
[JsonSerializable(typeof(RiskBreakdown))]
[JsonSerializable(typeof(TtfAnalysis))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class OracleJsonContext : JsonSerializerContext { }
```

---

## 1. Domain Models (Blueprint: record types)

```csharp
public record OracleRequest
{
    // Route factors
    public required string RouteId            { get; init; }
    public required string OriginCountry      { get; init; }
    public required string DestinationCountry { get; init; }
    public required RouteRiskCategory RouteCategory { get; init; }

    // Carrier factors
    public required Guid   CarrierId          { get; init; }
    public required double CarrierReliability { get; init; } // 0.0–1.0 (historical on-time rate)
    public required int    CarrierIncidents12M { get; init; } // cold chain incidents last 12 months

    // Environmental factors
    public required double ForecastMaxCelsius { get; init; } // worst-case ambient temp en route
    public required double ForecastMinCelsius { get; init; }
    public required double ForecastHumidityPct { get; init; }

    // Cargo / packaging
    public required string InsulationType     { get; init; } // "Active" | "Passive"
    public required string CargoType          { get; init; } // "Pharma" | "Food" | "Chemical"
    public required double SetpointMinCelsius { get; init; }
    public required double SetpointMaxCelsius { get; init; }

    // Physical TTF from thermal model (AHS.Engines.HPC output)
    public required double PhysicalTtfHours   { get; init; }
}

public enum RouteRiskCategory
{
    Low = 1,      // Direct, controlled, known route
    Medium = 2,   // One transhipment, moderate climate exposure
    High = 3,     // Multiple legs, extreme climate zones
    Critical = 4  // Remote, customs delays expected, conflict zones
}

public record RiskBreakdown(
    double RouteScore,          // 0–100
    double CarrierScore,        // 0–100
    double EnvironmentScore,    // 0–100
    double PassivePenalty,      // 0 or 15
    double WeightedRiskScore,   // 0–100 (pre-penalty weighted)
    double FinalRiskScore);     // 0–100 (post-penalty, capped)

public record TtfAnalysis(
    double PhysicalTtfHours,
    double PessimisticTtfHours,
    double SafeWindowHours,      // PessimisticTTF × 0.8 — recommended intervention threshold
    string RiskBand);            // "Green" | "Amber" | "Red" | "Critical"

public record OracleResult(
    Guid          RequestId,
    DateTimeOffset CalculatedAt,
    RiskBreakdown  Risk,
    TtfAnalysis    Ttf,
    XaiDna         Dna,
    string         Recommendation,
    bool           RequiresImmediateAction);
```

---

## 2. The Oracle Engine (AOT-safe, no reflection)

```csharp
public class LogisticsOracle(IRouteRepository routes, ICarrierRepository carriers,
    IWeatherService weather, ILogger<LogisticsOracle> logger)
{
    // Blueprint weights — sealed constants
    private const double W_Route       = 0.40;
    private const double W_Carrier     = 0.30;
    private const double W_Environment = 0.30;
    private const double PassivePenalty = 15.0;

    public async Task<OracleResult> CalculateAsync(OracleRequest req, CancellationToken ct)
    {
        // 1. Score each factor (0–100, higher = more risk)
        var routeScore  = ScoreRoute(req);
        var carrierScore = ScoreCarrier(req);
        var envScore    = ScoreEnvironment(req);

        // 2. Weighted composite
        var weighted = routeScore  * W_Route
                     + carrierScore * W_Carrier
                     + envScore    * W_Environment;

        // 3. Critical modifier — Passive Insulation
        var passivePenalty = req.InsulationType == "Passive" ? PassivePenalty : 0.0;

        // 4. Final score (capped at 100)
        var finalScore = Math.Min(100.0, weighted + passivePenalty);

        var breakdown = new RiskBreakdown(routeScore, carrierScore, envScore,
            passivePenalty, weighted, finalScore);

        // 5. Pessimistic TTF
        var ttf = CalculatePessimisticTtf(req.PhysicalTtfHours, finalScore);

        // 6. XAI DNA — 14-point diagnostic
        var dna = BuildXaiDna(req, breakdown, ttf);

        var result = new OracleResult(
            RequestId:  Guid.NewGuid(),
            CalculatedAt: DateTimeOffset.UtcNow,
            Risk:       breakdown,
            Ttf:        ttf,
            Dna:        dna,
            Recommendation:          BuildRecommendation(finalScore, ttf),
            RequiresImmediateAction: finalScore >= 75 || ttf.SafeWindowHours < 4);

        logger.LogInformation(
            "Oracle [{Id}]: Risk={Score:F1} TTF={Ttf:F1}h SafeWindow={Safe:F1}h",
            result.RequestId, finalScore, ttf.PhysicalTtfHours, ttf.SafeWindowHours);

        return result;
    }

    // --- Factor Scorers ---

    private static double ScoreRoute(OracleRequest req) => req.RouteCategory switch
    {
        RouteRiskCategory.Low      => 10.0,
        RouteRiskCategory.Medium   => 35.0,
        RouteRiskCategory.High     => 65.0,
        RouteRiskCategory.Critical => 90.0,
        _ => 50.0
    };

    private static double ScoreCarrier(OracleRequest req)
    {
        // Reliability: 1.0 = perfect → 0 risk. 0.5 = 50% → 50 risk.
        var reliabilityScore = (1.0 - req.CarrierReliability) * 100.0;

        // Incident penalty: each incident in last 12M adds 8 points (capped at 40)
        var incidentPenalty = Math.Min(40.0, req.CarrierIncidents12M * 8.0);

        return Math.Min(100.0, reliabilityScore + incidentPenalty);
    }

    private static double ScoreEnvironment(OracleRequest req)
    {
        // Margin above setpoint
        var tempMarginHigh = req.ForecastMaxCelsius - req.SetpointMaxCelsius;
        var tempMarginLow  = req.SetpointMinCelsius - req.ForecastMinCelsius;
        var worstMargin    = Math.Max(tempMarginHigh, tempMarginLow);

        // Score: 0 margin = 50 risk, +10°C margin breach = 100 risk
        var tempScore = worstMargin <= 0
            ? Math.Max(0, 50.0 + worstMargin * 5.0)  // below setpoint danger = negative margin
            : Math.Min(100.0, 50.0 + worstMargin * 5.0);

        // Humidity: >80% adds penalty for some cargo types
        var humidityPenalty = req.CargoType == "Pharma" && req.ForecastHumidityPct > 80
            ? (req.ForecastHumidityPct - 80.0) * 0.5
            : 0.0;

        return Math.Min(100.0, tempScore + humidityPenalty);
    }

    // --- Pessimistic TTF ---

    private static TtfAnalysis CalculatePessimisticTtf(double physicalTtf, double riskScore)
    {
        // Risk score reduces physical TTF linearly
        // 0 risk = full TTF, 100 risk = 40% of TTF (conservative floor)
        var reductionFactor = 1.0 - (riskScore / 100.0 * 0.60);
        var pessimisticTtf  = physicalTtf * reductionFactor;
        var safeWindow      = pessimisticTtf * 0.80;  // 80% of pessimistic = intervention threshold

        var band = (pessimisticTtf, riskScore) switch
        {
            _ when riskScore >= 75 || pessimisticTtf < 4  => "Critical",
            _ when riskScore >= 50 || pessimisticTtf < 12 => "Red",
            _ when riskScore >= 25 || pessimisticTtf < 24 => "Amber",
            _                                             => "Green"
        };

        return new TtfAnalysis(physicalTtf, pessimisticTtf, safeWindow, band);
    }

    private static string BuildRecommendation(double riskScore, TtfAnalysis ttf) =>
        (riskScore, ttf.RiskBand) switch
        {
            _ when ttf.RiskBand == "Critical" =>
                "IMMEDIATE ACTION REQUIRED: Divert shipment or deploy emergency cooling. Safe window < 4h.",
            _ when ttf.RiskBand == "Red" =>
                "HIGH RISK: Notify quality team. Increase monitoring frequency to 15-min intervals.",
            _ when ttf.RiskBand == "Amber" =>
                "ELEVATED RISK: Monitor closely. Prepare contingency carrier.",
            _ =>
                "Nominal. Standard monitoring protocol applies."
        };
}
```

---

## 3. XAI DNA — 14-Point Diagnostic

```csharp
// Each factor is fully explainable — audit-ready for regulatory oversight
public record XaiDna(IReadOnlyList<XaiDnaFactor> Factors);

public record XaiDnaFactor(
    int    Index,       // 1–14
    string Category,   // "Route" | "Carrier" | "Environment" | "Cargo" | "Composite"
    string Name,
    double Score,       // 0–100
    double Weight,      // contribution weight
    double WeightedContribution,
    string Verdict,     // "Low" | "Medium" | "High" | "Critical"
    string Explanation, // human-readable, audit-ready
    bool   IsModifier); // true for penalties like Passive Insulation

public class XaiDnaBuilder
{
    private readonly List<XaiDnaFactor> _factors = [];
    private int _index = 1;

    public XaiDnaBuilder Add(string category, string name, double score, double weight,
        string explanation, bool isModifier = false)
    {
        _factors.Add(new XaiDnaFactor(
            Index:                _index++,
            Category:             category,
            Name:                 name,
            Score:                score,
            Weight:               weight,
            WeightedContribution: isModifier ? score : score * weight,
            Verdict:              score switch { >= 75 => "Critical", >= 50 => "High",
                                                >= 25 => "Medium",  _ => "Low" },
            Explanation:          explanation,
            IsModifier:           isModifier));
        return this;
    }

    public XaiDna Build() => new(_factors);
}

// Usage inside LogisticsOracle.BuildXaiDna()
private static XaiDna BuildXaiDna(OracleRequest req, RiskBreakdown risk, TtfAnalysis ttf)
{
    return new XaiDnaBuilder()
        // ROUTE GROUP (factors 1–4)
        .Add("Route", "Route category risk",
            risk.RouteScore, 0.40,
            $"Route classified as {req.RouteCategory} ({risk.RouteScore:F0}/100). " +
            $"{req.OriginCountry} → {req.DestinationCountry}.")

        .Add("Route", "Transhipment exposure",
            req.RouteCategory >= RouteRiskCategory.High ? 60 : 20, 0.10,
            req.RouteCategory >= RouteRiskCategory.High
                ? "Multiple transhipment points increase excursion exposure windows."
                : "Direct or single transhipment route — minimal exposure risk.")

        .Add("Route", "Customs delay probability",
            req.RouteCategory == RouteRiskCategory.Critical ? 80 : 25, 0.08,
            req.RouteCategory == RouteRiskCategory.Critical
                ? "High customs delay probability on critical routes — adds uncontrolled dwell time."
                : "Low customs delay probability on this route profile.")

        .Add("Route", "Infrastructure reliability",
            req.RouteCategory >= RouteRiskCategory.High ? 55 : 15, 0.07,
            "Cold chain infrastructure quality score for transit countries.")

        // CARRIER GROUP (factors 5–8)
        .Add("Carrier", "Carrier reliability index",
            (1.0 - req.CarrierReliability) * 100, 0.30,
            $"Carrier historical on-time rate: {req.CarrierReliability:P0}. " +
            $"Risk contribution: {risk.CarrierScore:F0}/100.")

        .Add("Carrier", "Incident history (12M)",
            Math.Min(100, req.CarrierIncidents12M * 20.0), 0.10,
            $"{req.CarrierIncidents12M} cold chain incident(s) recorded in last 12 months.")

        .Add("Carrier", "Equipment compliance",
            req.CarrierIncidents12M > 2 ? 70 : 20, 0.08,
            req.CarrierIncidents12M > 2
                ? "Multiple incidents suggest equipment compliance gaps."
                : "No significant equipment compliance concerns.")

        .Add("Carrier", "SOP adherence score",
            req.CarrierReliability < 0.8 ? 50 : 15, 0.07,
            $"Estimated SOP adherence based on reliability index.")

        // ENVIRONMENT GROUP (factors 9–11)
        .Add("Environment", "Ambient temperature forecast",
            risk.EnvironmentScore, 0.30,
            $"Forecast max: {req.ForecastMaxCelsius:F1}°C, min: {req.ForecastMinCelsius:F1}°C. " +
            $"Setpoint: {req.SetpointMinCelsius:F1}–{req.SetpointMaxCelsius:F1}°C.")

        .Add("Environment", "Humidity exposure",
            req.ForecastHumidityPct > 80 ? 60 : 15, 0.05,
            $"Forecast humidity: {req.ForecastHumidityPct:F0}%. " +
            (req.ForecastHumidityPct > 80 ? "Elevated — condensation risk for pharma." : "Within acceptable range."))

        .Add("Environment", "Seasonal risk factor",
            DateTime.UtcNow.Month is >= 6 and <= 8 ? 55 : 20, 0.05,
            "Seasonal temperature profile for transit period.")

        // CARGO / MODIFIER GROUP (factors 12–14)
        .Add("Cargo", "Insulation type",
            risk.PassivePenalty > 0 ? 100 : 0, 0.0,
            req.InsulationType == "Passive"
                ? $"PASSIVE insulation detected. +{risk.PassivePenalty:F0}% base penalty applied per AHS guardrail."
                : "Active insulation — no base penalty.",
            isModifier: true)

        .Add("Cargo", "Cargo sensitivity",
            req.CargoType == "Pharma" ? 75 : req.CargoType == "Chemical" ? 60 : 40, 0.05,
            $"{req.CargoType} cargo sensitivity profile applied.")

        .Add("Composite", "Pessimistic TTF adequacy",
            ttf.SafeWindowHours < 8 ? 90 : ttf.SafeWindowHours < 24 ? 50 : 15, 0.10,
            $"Physical TTF: {ttf.PhysicalTtfHours:F1}h → Pessimistic: {ttf.PessimisticTtfHours:F1}h → " +
            $"Safe window: {ttf.SafeWindowHours:F1}h. Band: {ttf.RiskBand}.")

        .Build();
}
```

---

## 4. Slope & TTF Integration (AHS.Engines.HPC bridge)

```csharp
// The Oracle consumes PhysicalTtfHours computed by the HPC thermal engine
// This is the bridge interface — HPC computes, Oracle adjusts for logistics risk

public interface IThermalEngine
{
    /// <summary>
    /// Computes physical Time to Failure (hours) using thermal physics + SIMD slope analysis.
    /// AHS.Engines.HPC implements this — AVX-512 accelerated.
    /// </summary>
    ValueTask<double> ComputePhysicalTtfAsync(
        double currentCelsius,
        double ambientCelsius,
        double setpointMaxCelsius,
        string insulationType,
        double payloadMassKg,
        CancellationToken ct);
}

// Full Oracle pipeline:
public class OraclePipeline(IThermalEngine hpc, LogisticsOracle oracle)
{
    public async Task<OracleResult> RunAsync(OracleRequest req, double currentTemp, CancellationToken ct)
    {
        // 1. Physical TTF from HPC (SIMD-accelerated thermal model)
        var physicalTtf = await hpc.ComputePhysicalTtfAsync(
            currentTemp,
            req.ForecastMaxCelsius,
            req.SetpointMaxCelsius,
            req.InsulationType,
            payloadMassKg: 100.0, // from shipment manifest
            ct);

        // 2. Oracle adjusts for logistics risk
        var reqWithTtf = req with { PhysicalTtfHours = physicalTtf };
        return await oracle.CalculateAsync(reqWithTtf, ct);
    }
}
```

---

## 5. Minimal API Endpoint (AOT-safe)

```csharp
// Program.cs
app.MapPost("/api/oracle/calculate", async (
    OracleRequest req,
    OraclePipeline pipeline,
    ICurrentTemperatureService tempSvc,
    CancellationToken ct) =>
{
    var currentTemp = await tempSvc.GetLatestAsync(req.RouteId, ct);
    var result = await pipeline.RunAsync(req, currentTemp, ct);

    return result.RequiresImmediateAction
        ? Results.Json(result, OracleJsonContext.Default.OracleResult, statusCode: 200)
        : Results.Json(result, OracleJsonContext.Default.OracleResult);
})
.WithName("CalculateRisk")
.WithSummary("REQ-001: Logistics Oracle risk assessment");
```

---

## 6. XAI DNA Blazor Component

```razor
@* XaiDnaPanel.razor — renders the 14-point diagnostic in the Command Console *@
@using AHS.Common.Oracle

<div class="xai-panel">
    <h3 class="xai-title">XAI DNA — Diagnostic Breakdown</h3>

    @foreach (var group in Dna.Factors.GroupBy(f => f.Category))
    {
        <div class="xai-group">
            <span class="xai-group-label">@group.Key</span>
            @foreach (var factor in group)
            {
                <div class="xai-factor @($"verdict-{factor.Verdict.ToLower()}")">
                    <span class="factor-index">@factor.Index</span>
                    <div class="factor-body">
                        <div class="factor-name">@factor.Name</div>
                        <div class="factor-bar">
                            <div class="factor-fill" style="width: @(factor.Score)%"></div>
                        </div>
                        <div class="factor-explanation">@factor.Explanation</div>
                    </div>
                    <span class="factor-score">@factor.Score.ToString("F0")</span>
                </div>
            }
        </div>
    }
</div>

@code {
    [Parameter] public required XaiDna Dna { get; set; }
}
```
