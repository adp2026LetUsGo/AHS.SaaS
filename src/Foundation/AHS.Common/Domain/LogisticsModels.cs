// src/Foundation/AHS.Common/Domain/LogisticsModels.cs
namespace AHS.Common.Domain;

public enum InsulationType { Passive, Active }

public record ShipmentRiskProfile {
    public string CarrierName { get; init; } = string.Empty;
    public float CarrierReliability { get; init; } // 0-1
    public string RouteId { get; init; } = string.Empty;
    public float RouteThreatLevel { get; init; }   // 0-1
    public InsulationType Insulation { get; init; }
    public float ExternalTempForecast { get; init; }

    public ShipmentRiskProfile() { }

    public ShipmentRiskProfile(string carrierName, float carrierReliability, string routeId, float routeThreatLevel, InsulationType insulation, float externalTempForecast) {
        CarrierName = carrierName;
        CarrierReliability = carrierReliability;
        RouteId = routeId;
        RouteThreatLevel = routeThreatLevel;
        Insulation = insulation;
        ExternalTempForecast = externalTempForecast;
    }
}

public record LogisticsOracleResult(
    float CompositeRiskScore, // 0-100%
    float PessimisticTTF,      // Adjusted minutes
    string RiskLevel,
    string PrimaryInsight = "NOMINAL_STABILITY"
);
