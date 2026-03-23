namespace AHS.Common.Models;

public enum InsulationType { Passive, Active }

public class ShipmentRiskProfile {
    public string CarrierName { get; set; } = string.Empty;
    public float CarrierReliability { get; set; } // 0-1
    public string RouteId { get; set; } = string.Empty;
    public float RouteThreatLevel { get; set; }   // 0-1
    public InsulationType Insulation { get; set; }
    public float ExternalTempForecast { get; set; }

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
