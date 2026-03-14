namespace AHS.Common.Models;

public enum InsulationType { Passive, Active }

public record ShipmentRiskProfile(
    string CarrierName,
    float CarrierReliability, // 0-1
    string RouteId,
    float RouteThreatLevel,   // 0-1
    InsulationType Insulation,
    float ExternalTempForecast
);

public record LogisticsOracleResult(
    float CompositeRiskScore, // 0-100%
    float PessimisticTTF,      // Adjusted minutes
    string RiskLevel
);
