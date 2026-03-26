// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Oracle/LogisticsOracle.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AHS.Cell.Xinfer.Domain.Enums;
using AHS.Common.Domain;
using InsulationType = AHS.Cell.Xinfer.Domain.Enums.InsulationType;

namespace AHS.Cell.Xinfer.Application.Oracle;

public sealed class LogisticsOracle
{
    /// <summary>
    /// REQ-001: P99 target < 10ms. No LINQ, no heap allocations in hot path.
    /// </summary>
    public static ValueTask<OracleResult> CalculateAsync(OracleRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);
        // 1. Calculate weighted Risk Score
        // Route Category: Critical=1.0, Elevated=0.6, Nominal=0.2
        float routeWeight = req.RouteCategory switch
        {
            "Critical" => 1.0f,
            "Elevated" => 0.6f,
            _ => 0.2f
        };

        float riskScore = (routeWeight * 0.4f) + 
                          ((1.0f - req.CarrierReliability) * 0.3f) + 
                          (Math.Clamp(req.ForecastMaxCelsius / 40.0f, 0, 1) * 0.3f);

        // 2. Apply passive insulation modifier
        if (req.InsulationType == InsulationType.Passive)
        {
            riskScore = Math.Clamp(riskScore + 0.15f, 0, 1.0f);
        }

        float riskPercent = riskScore * 100.0f;

        // 3. Pessimistic TTF: PhysicalTtf × (1 - riskScore × 0.60)
        float pessimisticTtf = req.PhysicalTtfHours * (1.0f - (riskScore * 0.60f));
        float safeWindow = pessimisticTtf * 0.80f;

        // 4. Construct XAI DNA (14 factors)
        var dna = new XaiDna(
            RouteRisk: routeWeight,
            CarrierRisk: 1.0f - req.CarrierReliability,
            WeatherRisk: Math.Clamp(req.ForecastMaxCelsius / 40.0f, 0, 1),
            InsulationPenalty: req.InsulationType == InsulationType.Passive ? 0.15f : 0.0f,
            HumidityFactor: req.ForecastHumidityPct / 100.0f,
            HistoricalReliability: req.CarrierReliability,
            SeasonalVariance: 0.1f, // Placeholder for actual temporal analysis
            TransitVolatility: req.CarrierIncidents12M / 10.0f,
            SetpointProximity: Math.Abs(req.SetpointMaxCelsius - req.ForecastMaxCelsius) / 10.0f,
            AmbientDelta: req.ForecastMaxCelsius - req.SetpointMaxCelsius,
            GeopoliticalThreat: 0.05f,
            InfrastructureScore: 0.9f,
            EnergyStability: 1.0f,
            AuditConfidence: 1.0f
        );

        // 5. Recommendation Logic
        string recommendation = riskPercent > 75 ? "ABORT_SHIPMENT" :
                               riskPercent > 40 ? "USE_ACTIVE_INSULATION" : "PROCEED_NOMINAL";

        bool immediateAction = riskPercent > 75 || pessimisticTtf < 4.0f;

        var result = new OracleResult(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            riskPercent,
            pessimisticTtf,
            safeWindow,
            dna,
            recommendation,
            immediateAction
        );

        return new ValueTask<OracleResult>(result);
    }
}
