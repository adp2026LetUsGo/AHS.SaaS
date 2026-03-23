// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Oracle/XaiDna.cs
namespace AHS.Cell.ColdChain.Application.Oracle;

/// <summary>
/// Exactly 14 diagnostic factors per Blueprint contract.
/// </summary>
public readonly record struct XaiDna(
    float RouteRisk,
    float CarrierRisk,
    float WeatherRisk,
    float InsulationPenalty,
    float HumidityFactor,
    float HistoricalReliability,
    float SeasonalVariance,
    float TransitVolatility,
    float SetpointProximity,
    float AmbientDelta,
    float GeopoliticalThreat,
    float InfrastructureScore,
    float EnergyStability,
    float AuditConfidence
);
